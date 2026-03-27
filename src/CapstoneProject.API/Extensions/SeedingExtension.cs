using System.IO;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CapstoneProject.Infrastructure.Context;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.API.Extensions;

public static class SeedingExtension
{
    public static async Task SeedInitialDataAsync(this IApplicationBuilder app, ILogger logger)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var enableSeeding = configuration.GetSection("DataSeeding").GetValue<bool>("EnableSeeding");
        if (!enableSeeding)
        {
            logger.LogInformation("Data seeding is disabled.");
            return;
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CapstoneProjectDbContext>();

        // Migrate legacy "Student" role to "Learner" first (one-time fix for DBs created before rename)
        var studentRole = await roleManager.FindByNameAsync("Student");
        if (studentRole != null)
        {
            studentRole.Name = "Learner";
            studentRole.NormalizedName = "LEARNER";
            var updateResult = await roleManager.UpdateAsync(studentRole);
            if (updateResult.Succeeded)
            {
                logger.LogInformation("Migrated role Student to Learner.");
            }
            else
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to migrate Student to Learner: {Errors}", errors);
            }
        }

        // Seed roles from RoleEnum (Admin, Learner, Moderator)
        foreach (var roleName in Enum.GetNames(typeof(RoleEnum)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Status = EntityStatusEnum.Active,
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
                };
                var roleResult = await roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create role {Role}: {Errors}", roleName, errors);
                }
                else
                {
                    logger.LogInformation("Created role: {Role}", roleName);
                }
            }
        }

        // Seed admin user
        var adminEmail = configuration.GetSection("AdminUser").GetValue<string>("Email")?.Trim();
        var adminPassword = configuration.GetSection("AdminUser").GetValue<string>("DefaultPassword");
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("AdminUser configuration is missing. Skipping admin seeding.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                JoiningAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to create admin user: {Errors}", errors);
                return;
            }
            existingAdmin = admin;
            logger.LogInformation("Created admin user {Email}", adminEmail);
        }

        // Ensure admin is in Admin role
        var adminRoleName = RoleEnum.Admin.ToString();

        if (!await userManager.IsInRoleAsync(existingAdmin, adminRoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(existingAdmin, adminRoleName);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to add admin user to role {Role}: {Errors}", adminRoleName, errors);
            }
            else
            {
                logger.LogInformation("Added admin user to role {Role}", adminRoleName);
            }
        }

        // Final verification
        var roles = await userManager.GetRolesAsync(existingAdmin);
        logger.LogInformation("Admin user {Email} roles: {Roles}", existingAdmin.Email, string.Join(", ", roles));

        // Seed demo user for quick login
        var demoUserEmail = configuration.GetSection("QuickLogin").GetValue<string>("DemoUserEmail");
        var demoUserPassword = configuration.GetSection("QuickLogin").GetValue<string>("DemoUserPassword") ?? "Demo@123";
        
        if (!string.IsNullOrWhiteSpace(demoUserEmail))
        {
            var existingDemoUser = await userManager.FindByEmailAsync(demoUserEmail);
            if (existingDemoUser == null)
            {
                var demoUser = new AppUser
                {
                    UserName = demoUserEmail,
                    Email = demoUserEmail,
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "User",
                    JoiningAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    Status = EntityStatusEnum.Active
                };

                var createDemoResult = await userManager.CreateAsync(demoUser, demoUserPassword);
                if (!createDemoResult.Succeeded)
                {
                    var errors = string.Join(", ", createDemoResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create demo user: {Errors}", errors);
                }
                else
                {
                    existingDemoUser = demoUser;
                    logger.LogInformation("Created demo user {Email}", demoUserEmail);
                }
            }

            // Ensure demo user is in Learner role
            if (existingDemoUser != null)
            {
                var learnerRoleName = RoleEnum.Learner.ToString();
                if (!await userManager.IsInRoleAsync(existingDemoUser, learnerRoleName))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(existingDemoUser, learnerRoleName);
                    if (!addRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                        logger.LogWarning("Failed to add demo user to role {Role}: {Errors}", learnerRoleName, errors);
                    }
                    else
                    {
                        logger.LogInformation("Added demo user to role {Role}", learnerRoleName);
                    }
                }
            }
        }

        // Seed map tags (idempotent)
        var defaultTagNames = new[]
        {
            "Variables",
            "Operators",
            "Conditionals",
            "Loops",
            "Functions",
            "Arrays",
            "Objects",
            "Pointers",
            "Recursion",
            "Algorithm Basics",
            "Beginner",
            "Easy",
            "Medium",
            "Hard",
            "Expert",
            "Pathfinding",
            "Resource Collection",
            "Obstacle Avoidance",
            "Logic Puzzle",
            "Optimization",
            "Pattern Recognition",
            "Strategy",
            "Logical Thinking",
            "Problem Solving",
            "Computational Thinking",
            "Algorithm Design",
            "Debugging"
        };

        var existingTagNames = await dbContext.Tags
            .AsNoTracking()
            .Select(t => t.Name)
            .ToListAsync();
        var existingTagSet = new HashSet<string>(existingTagNames, StringComparer.OrdinalIgnoreCase);

        var tagsToInsert = defaultTagNames
            .Where(name => !existingTagSet.Contains(name))
            .Select(name => new Tag
            {
                Name = name,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = existingAdmin?.Id ?? Guid.Empty,
                Status = EntityStatusEnum.Active
            })
            .ToList();

        if (tagsToInsert.Count > 0)
        {
            await dbContext.Tags.AddRangeAsync(tagsToInsert);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} tags.", tagsToInsert.Count);
        }
        else
        {
            logger.LogInformation("All default tags already exist.");
        }

        // Seed membership packages (upsert, VND prices)
        var packageSeeds = new[]
        {
            new Package
            {
                Name = "Free",
                DurationDays = 3650,
                Limit = 20,
                Price = 0,
                FeaturesSpec = "Play basic maps; max 20 maps; no hints; cannot create/publish maps; no XP boost."
            },
            new Package
            {
                Name = "Pro",
                DurationDays = 30,
                Limit = null,
                Price = 149m,
                FeaturesSpec = "Play basic and advanced maps; hints enabled; cannot create/publish maps; XP boost enabled."
            },
            new Package
            {
                Name = "Creator",
                DurationDays = 30,
                Limit = null,
                Price = 299m,
                FeaturesSpec = "Play basic and advanced maps; hints enabled; can create and publish maps; map analytics; XP boost enabled."
            }
        };

        var existingPackages = await dbContext.Packages
            .Where(p => packageSeeds.Select(s => s.Name).Contains(p.Name))
            .ToListAsync();

        foreach (var seed in packageSeeds)
        {
            var existing = existingPackages.FirstOrDefault(p => p.Name.Equals(seed.Name, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                seed.CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                seed.CreatedBy = existingAdmin?.Id ?? Guid.Empty;
                seed.Status = EntityStatusEnum.Active;
                await dbContext.Packages.AddAsync(seed);
            }
            else
            {
                existing.DurationDays = seed.DurationDays;
                existing.Limit = seed.Limit;
                existing.Price = seed.Price;
                existing.FeaturesSpec = seed.FeaturesSpec;
                existing.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                existing.UpdatedBy = existingAdmin?.Id ?? Guid.Empty;
                if (existing.Status != EntityStatusEnum.Active)
                    existing.Status = EntityStatusEnum.Active;
            }
        }

        await dbContext.SaveChangesAsync();

        // Seed payment methods: OrbitCoin, PayOS
        var orbitCoinPayment = await dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Code == "OrbitCoin");
        if (orbitCoinPayment == null)
        {
            var payment = new Payment
            {
                Code = "OrbitCoin",
                Name = "OrbitCoin",
                Description = "Virtual currency (in-platform)",
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = existingAdmin?.Id ?? Guid.Empty,
                Status = EntityStatusEnum.Active
            };
            await dbContext.Payments.AddAsync(payment);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded payment method: OrbitCoin.");
        }

        var payOSPayment = await dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Code == "PayOS");
        if (payOSPayment == null)
        {
            var payos = new Payment
            {
                Code = "PayOS",
                Name = "PayOS",
                Description = "User top-up via PayOS",
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = existingAdmin?.Id ?? Guid.Empty,
                Status = EntityStatusEnum.Active
            };
            await dbContext.Payments.AddAsync(payos);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded payment method: PayOS.");
        }

        // Seed maps from SQL file (INSERT Maps/MapDetails/Hints/MapTags), toggled by DataSeeding:SeedMapsFromSqlScript.
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var seedMapsFromSqlScript = configuration.GetSection("DataSeeding").GetValue<bool>("SeedMapsFromSqlScript");
        if (seedMapsFromSqlScript)
        {
            // Neon/Postgres pooler hosts (often containing "-pooler") can still run SQL seeding.
            var relativeScriptPath = configuration.GetSection("DataSeeding").GetValue<string>("MapsSqlScriptPath")?.Trim();
            var scriptPath = !string.IsNullOrWhiteSpace(relativeScriptPath)
                ? Path.GetFullPath(Path.Combine(env.ContentRootPath, relativeScriptPath))
                : Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "docs", "script_clean.sql"));

            var systemUserId = existingAdmin?.Id ?? Guid.Empty;
            await SeedMapsFromSqlScriptAsync(dbContext, scriptPath, systemUserId, logger);
        }
        if (!seedMapsFromSqlScript)
        {
            logger.LogInformation("Map seeding from SQL script is disabled (DataSeeding:SeedMapsFromSqlScript=false).");
        }

        // Seed Learning Goals (idempotent by Name).
        var learningGoalSeeds = new[]
        {
            new { Name = "Logic cơ bản", Description = "Làm quen với biến, phép toán, thứ tự thực thi và điều khiển luồng cơ bản.", SortOrder = 1 },
            new { Name = "Điều kiện", Description = "Học cách dùng if/else, so sánh và rẽ nhánh trong chương trình.", SortOrder = 2 },
            new { Name = "Vòng lặp", Description = "Làm chủ for, while và xử lý lặp để giải quyết bài toán.", SortOrder = 3 },
            new { Name = "Giải quyết vấn đề", Description = "Kết hợp logic, điều kiện và vòng lặp để phân tích và giải bài toán.", SortOrder = 4 }
        };

        foreach (var seed in learningGoalSeeds)
        {
            var existingGoal = await dbContext.LearningGoals
                .FirstOrDefaultAsync(g => !g.IsDeleted && g.Name == seed.Name);

            if (existingGoal == null)
            {
                var goal = new LearningGoal
                {
                    Name = seed.Name,
                    Description = seed.Description,
                    SortOrder = seed.SortOrder,
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    CreatedBy = existingAdmin?.Id,
                    Status = EntityStatusEnum.Active
                };
                goal.InitializeEntity(existingAdmin?.Id);
                await dbContext.LearningGoals.AddAsync(goal);
                logger.LogInformation("Seeded learning goal: {Name}", seed.Name);
            }
            else
            {
                existingGoal.Description = seed.Description;
                existingGoal.SortOrder = seed.SortOrder;
                existingGoal.Status = EntityStatusEnum.Active;
                existingGoal.UpdateEntity(existingAdmin?.Id);
                logger.LogInformation("Updated learning goal: {Name}", seed.Name);
            }
        }
        await dbContext.SaveChangesAsync();

        // Seed Concepts (idempotent by LearningGoalId + Name)
        var goalsByName = await dbContext.LearningGoals
            .Where(g => !g.IsDeleted)
            .ToDictionaryAsync(g => g.Name, g => g.Id, StringComparer.OrdinalIgnoreCase);

        var conceptSeeds = new[]
        {
            // Logic cơ bản
            (GoalName: "Logic cơ bản", Name: "Biến là gì", Description: "Làm quen với biến và gán giá trị.", ContentKey: "variables", SortOrder: 1),
            (GoalName: "Logic cơ bản", Name: "Phép toán", Description: "Các phép toán cơ bản: cộng, trừ, nhân, chia.", ContentKey: "operators", SortOrder: 2),
            (GoalName: "Logic cơ bản", Name: "Thứ tự thực thi", Description: "Chương trình chạy từ trên xuống dưới, từ trái sang phải.", ContentKey: "execution-order", SortOrder: 3),
            // Điều kiện
            (GoalName: "Điều kiện", Name: "If-else", Description: "Rẽ nhánh theo điều kiện đúng/sai.", ContentKey: "if-else", SortOrder: 1),
            (GoalName: "Điều kiện", Name: "So sánh", Description: "So sánh lớn hơn, nhỏ hơn, bằng.", ContentKey: "comparison", SortOrder: 2),
            // Vòng lặp
            (GoalName: "Vòng lặp", Name: "For loop", Description: "Vòng lặp với số lần xác định.", ContentKey: "for-loop", SortOrder: 1),
            (GoalName: "Vòng lặp", Name: "While loop", Description: "Vòng lặp khi điều kiện còn đúng.", ContentKey: "while-loop", SortOrder: 2),
            // Giải quyết vấn đề
            (GoalName: "Giải quyết vấn đề", Name: "Phân tích bài toán", Description: "Đọc đề, tìm input/output, chia bước.", ContentKey: "problem-analysis", SortOrder: 1),
            (GoalName: "Giải quyết vấn đề", Name: "Thuật toán cơ bản", Description: "Các bước giải quyết bài toán bằng code.", ContentKey: "basic-algorithm", SortOrder: 2)
        };

        foreach (var (goalName, name, description, contentKey, sortOrder) in conceptSeeds)
        {
            if (!goalsByName.TryGetValue(goalName, out var goalId))
                continue;

            var existingConcept = await dbContext.Concepts
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.LearningGoalId == goalId && c.Name == name);

            if (existingConcept == null)
            {
                var concept = new Concept
                {
                    LearningGoalId = goalId,
                    Name = name,
                    Description = description,
                    ContentKey = contentKey,
                    SortOrder = sortOrder,
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    CreatedBy = existingAdmin?.Id,
                    Status = EntityStatusEnum.Active
                };
                concept.InitializeEntity(existingAdmin?.Id);
                await dbContext.Concepts.AddAsync(concept);
                logger.LogInformation("Seeded concept: {GoalName} / {Name}", goalName, name);
            }
            else
            {
                existingConcept.Description = description;
                existingConcept.ContentKey = contentKey;
                existingConcept.SortOrder = sortOrder;
                existingConcept.Status = EntityStatusEnum.Active;
                existingConcept.UpdateEntity(existingAdmin?.Id);
                logger.LogInformation("Updated concept: {GoalName} / {Name}", goalName, name);
            }
        }
        await dbContext.SaveChangesAsync();

        // Seed LearningPathItems (idempotent: skip if (LearningGoalId, SortOrder) already exists).
        var existingPathItemKeys = await dbContext.LearningPathItems
            .Where(i => !i.IsDeleted)
            .Select(i => new { i.LearningGoalId, i.SortOrder })
            .ToListAsync();
        var existingPathItemSet = new HashSet<(Guid, int)>(existingPathItemKeys.Select(k => (k.LearningGoalId, k.SortOrder)));

        var conceptIdsByGoalAndName = await dbContext.Concepts
            .Where(c => !c.IsDeleted)
            .Select(c => new { c.LearningGoalId, c.Name, c.Id })
            .ToListAsync();
        var conceptIdLookup = conceptIdsByGoalAndName
            .GroupBy(x => x.LearningGoalId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase));

        // Title must match Maps.Title (for example from script_clean.sql / published maps). Map assignment follows concept flow.
        var mapTitles = new[]
        {
            "Introduce variable",
            "Mathematical operation",
            "Platform movement tutorial",
            "Introduce trap",
            "More Box",
            "Introduce for loop",
            "Introduce while/do while loop",
            "Basic top down map",
            "Maze map",
            // Legacy fallbacks (if old DB only has legacy map titles).
            "level-platform-01",
            "level-topdown-1771989668367",
            "level-topdown-foreground-example"
        };
        var mapsInList = await dbContext.Maps
            .Where(m => !m.IsDeleted && mapTitles.Contains(m.Title))
            .Select(m => new { m.Title, m.Id })
            .ToListAsync();
        var mapIdsByTitle = mapsInList
            .GroupBy(m => m.Title, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        static string? PickMapTitle(
            string primary,
            string fallbackLegacy1,
            string fallbackLegacy2,
            IReadOnlyDictionary<string, Guid> byTitle)
        {
            if (byTitle.ContainsKey(primary)) return primary;
            if (byTitle.ContainsKey(fallbackLegacy1)) return fallbackLegacy1;
            if (byTitle.ContainsKey(fallbackLegacy2)) return fallbackLegacy2;
            return null;
        }

        // (GoalName, ItemType, ConceptName?, MapTitle?, SortOrder) with preferred new map title and legacy fallback.
        var pathItemSeeds = new[]
        {
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Biến là gì", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce variable", "level-platform-01", "level-topdown-1771989668367", mapIdsByTitle), SortOrder: 2),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Phép toán", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Mathematical operation", "level-topdown-1771989668367", "level-platform-01", mapIdsByTitle), SortOrder: 4),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Thứ tự thực thi", MapTitle: (string?)null, SortOrder: 5),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Platform movement tutorial", "level-topdown-foreground-example", "level-platform-01", mapIdsByTitle), SortOrder: 6),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "If-else", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce trap", "level-platform-01", "level-topdown-1771989668367", mapIdsByTitle), SortOrder: 2),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "So sánh", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("More Box", "level-topdown-1771989668367", "level-platform-01", mapIdsByTitle), SortOrder: 4),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "For loop", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce for loop", "level-platform-01", "level-topdown-1771989668367", mapIdsByTitle), SortOrder: 2),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "While loop", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce while/do while loop", "level-topdown-1771989668367", "level-platform-01", mapIdsByTitle), SortOrder: 4),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Phân tích bài toán", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Basic top down map", "level-topdown-1771989668367", "level-platform-01", mapIdsByTitle), SortOrder: 2),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Thuật toán cơ bản", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Map, ConceptName: (string?)null, MapTitle: PickMapTitle("Maze map", "level-platform-01", "level-topdown-foreground-example", mapIdsByTitle), SortOrder: 4)
        };

        foreach (var (goalName, itemType, conceptName, mapTitle, sortOrder) in pathItemSeeds)
        {
            if (!goalsByName.TryGetValue(goalName, out var goalId))
                continue;
            Guid? conceptId = null;
            Guid? mapId = null;
            if (itemType == LearningPathItemTypeEnum.Concept && !string.IsNullOrEmpty(conceptName) && conceptIdLookup.TryGetValue(goalId, out var byName) && byName.TryGetValue(conceptName, out var cId))
                conceptId = cId;
            if (itemType == LearningPathItemTypeEnum.Map && !string.IsNullOrEmpty(mapTitle) && mapIdsByTitle.TryGetValue(mapTitle, out var mId))
                mapId = mId;

            var existingPathItem = await dbContext.LearningPathItems
                .FirstOrDefaultAsync(i => !i.IsDeleted && i.LearningGoalId == goalId && i.SortOrder == sortOrder);

            if (existingPathItem == null)
            {
                var item = new LearningPathItem
                {
                    LearningGoalId = goalId,
                    ItemType = itemType,
                    ConceptId = conceptId,
                    MapId = mapId,
                    SortOrder = sortOrder,
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    CreatedBy = existingAdmin?.Id,
                    Status = EntityStatusEnum.Active
                };
                item.InitializeEntity(existingAdmin?.Id);
                await dbContext.LearningPathItems.AddAsync(item);
                logger.LogInformation("Seeded path item: {GoalName} SortOrder={SortOrder} {Type}", goalName, sortOrder, itemType);
            }
            else
            {
                existingPathItem.ItemType = itemType;
                existingPathItem.ConceptId = itemType == LearningPathItemTypeEnum.Concept ? conceptId : null;
                // Do not erase an existing MapId when current title lookup fails.
                if (itemType == LearningPathItemTypeEnum.Map && mapId.HasValue)
                    existingPathItem.MapId = mapId;
                else if (itemType == LearningPathItemTypeEnum.Concept)
                    existingPathItem.MapId = null;

                existingPathItem.Status = EntityStatusEnum.Active;
                existingPathItem.UpdateEntity(existingAdmin?.Id);
                logger.LogInformation("Updated path item: {GoalName} SortOrder={SortOrder} {Type}", goalName, sortOrder, itemType);
            }
        }
        await dbContext.SaveChangesAsync();

        await SeedXpConfigurationDataAsync(dbContext, existingAdmin?.Id, logger);

        logger.LogInformation("Data seeding completed.");
    }

    private static async Task SeedXpConfigurationDataAsync(CapstoneProjectDbContext dbContext, Guid? userId, ILogger logger)
    {
        var actorId = userId ?? Guid.Empty;
        var now = VietnamDateTime.DbNow;

        var levelThresholdSeeds = new[]
        {
            new { Level = 1, RequiredTotalXp = 0, Title = "Beginner" },
            new { Level = 2, RequiredTotalXp = 500, Title = "Novice" },
            new { Level = 3, RequiredTotalXp = 1200, Title = "Skilled" },
            new { Level = 4, RequiredTotalXp = 2500, Title = "Advanced" },
            new { Level = 5, RequiredTotalXp = 4500, Title = "Expert" }
        };

        foreach (var seed in levelThresholdSeeds)
        {
            var existing = await dbContext.LevelThresholds.FirstOrDefaultAsync(x => !x.IsDeleted && x.Level == seed.Level);
            if (existing == null)
            {
                var row = new LevelThreshold
                {
                    Level = seed.Level,
                    RequiredTotalXp = seed.RequiredTotalXp,
                    Title = seed.Title,
                    Status = EntityStatusEnum.Active
                };
                row.InitializeEntity(actorId);
                await dbContext.LevelThresholds.AddAsync(row);
            }
            else
            {
                existing.RequiredTotalXp = seed.RequiredTotalXp;
                existing.Title = seed.Title;
                existing.UpdateEntity(actorId);
            }
        }

        var policySeeds = new[]
        {
            new { Key = "BaseRewardPolicy", Enabled = true, Priority = 10, ConfigJson = "{\"enabled\":true}" },
            new { Key = "DailyCapPolicy", Enabled = true, Priority = 20, ConfigJson = "{\"globalDailyCap\":300}" },
            new { Key = "BonusPolicy", Enabled = true, Priority = 30, ConfigJson = "{\"weekendMultiplier\":1.5}" },
            new { Key = "StreakPolicy", Enabled = true, Priority = 40, ConfigJson = "{\"minDays\":3,\"bonusXp\":20,\"maxBonusXp\":100}" },
            new { Key = "FirstWinOfDayPolicy", Enabled = true, Priority = 50, ConfigJson = "{\"bonusXp\":15}" },
            new { Key = "EventBoostPolicy", Enabled = false, Priority = 60, ConfigJson = "{\"multiplier\":2,\"eventCode\":\"launch-week\"}" }
        };

        foreach (var seed in policySeeds)
        {
            var existing = await dbContext.XpPolicyConfigs.FirstOrDefaultAsync(x => !x.IsDeleted && x.PolicyKey == seed.Key);
            if (existing == null)
            {
                var row = new XpPolicyConfig
                {
                    PolicyKey = seed.Key,
                    IsEnabled = seed.Enabled,
                    Priority = seed.Priority,
                    ConfigJson = seed.ConfigJson,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Status = EntityStatusEnum.Active
                };
                row.InitializeEntity(actorId);
                await dbContext.XpPolicyConfigs.AddAsync(row);
            }
            else
            {
                existing.IsEnabled = seed.Enabled;
                existing.Priority = seed.Priority;
                existing.ConfigJson = seed.ConfigJson;
                existing.UpdateEntity(actorId);
            }
        }

        var sourceSeeds = new[]
        {
            new { Source = XpSourceTypeEnum.MapSolve, Enabled = true, BaseXp = 10, DailyCap = 0, Bonus = 1.0, Config = (string?)null },
            new { Source = XpSourceTypeEnum.ConceptComplete, Enabled = true, BaseXp = 30, DailyCap = 120, Bonus = 1.0, Config = (string?)null },
            new { Source = XpSourceTypeEnum.LearningPathComplete, Enabled = true, BaseXp = 50, DailyCap = 150, Bonus = 1.0, Config = (string?)null },
            new { Source = XpSourceTypeEnum.AdminGrant, Enabled = true, BaseXp = 0, DailyCap = 0, Bonus = 1.0, Config = (string?)null },
            new { Source = XpSourceTypeEnum.XpBonus, Enabled = true, BaseXp = 0, DailyCap = 0, Bonus = 1.0, Config = (string?)null }
        };

        foreach (var seed in sourceSeeds)
        {
            var existing = await dbContext.XpSourceConfigs.FirstOrDefaultAsync(x => !x.IsDeleted && x.SourceType == seed.Source);
            if (existing == null)
            {
                var row = new XpSourceConfig
                {
                    SourceType = seed.Source,
                    IsEnabled = seed.Enabled,
                    BaseXp = seed.BaseXp,
                    DailyCap = seed.DailyCap,
                    BonusMultiplier = seed.Bonus,
                    ConfigJson = seed.Config,
                    Status = EntityStatusEnum.Active
                };
                row.InitializeEntity(actorId);
                await dbContext.XpSourceConfigs.AddAsync(row);
            }
            else
            {
                existing.IsEnabled = seed.Enabled;
                existing.BaseXp = seed.BaseXp;
                existing.DailyCap = seed.DailyCap;
                existing.BonusMultiplier = seed.Bonus;
                existing.ConfigJson = seed.Config;
                existing.UpdateEntity(actorId);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("XP configuration seeding completed.");
    }

    private sealed class ConceptKeyComparer : IEqualityComparer<(Guid, string)>
    {
        public bool Equals((Guid, string) x, (Guid, string) y) => x.Item1 == y.Item1 && string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode((Guid, string) obj) => HashCode.Combine(obj.Item1, obj.Item2.GetHashCode(StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>GUID user literal in script_clean.sql for CreatedBy/UpdatedBy. Replaced by systemUserId during seed.</summary>
    private const string ScriptCreatedByUserIdLiteral = "29f8c7e0-11bb-46c1-327b-08de83cfc02d";

    private static async Task SeedMapsFromSqlScriptAsync(CapstoneProjectDbContext dbContext, string scriptPath, Guid systemUserId, ILogger logger)
    {
        if (!File.Exists(scriptPath))
        {
            logger.LogWarning("Maps SQL script not found: {Path}", scriptPath);
            return;
        }

        // INSERT data only; no DDL. Existing seeded Tags are reused and MapTags are remapped by tag Name.
        var allowedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Maps",
            "MapDetails",
            "Hints",
            "MapTags"
        };

        var sourceOrder = 0;
        var inserts = ExtractInsertStatements(scriptPath, allowedTables)
            .Select(s =>
            {
                s.SourceOrder = sourceOrder++;
                return s;
            })
            .ToList();
        if (inserts.Count == 0)
        {
            logger.LogWarning("No INSERT statements found for allowed tables in: {Path}", scriptPath);
            return;
        }

        // Map TagId from script to current Tag Id in DB by Name.
        var scriptTagIdToName = ExtractScriptTagIdToName(scriptPath);
        var nameToCurrentTagId = await dbContext.Tags
            .Where(t => !t.IsDeleted)
            .ToDictionaryAsync(t => t.Name, t => t.Id.ToString("D"), StringComparer.OrdinalIgnoreCase);
        var scriptTagIdToCurrentId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (scriptId, name) in scriptTagIdToName)
        {
            if (nameToCurrentTagId.TryGetValue(name, out var currentId))
                scriptTagIdToCurrentId[scriptId] = currentId;
        }

        // Execution order: Maps, MapDetails, Hints, MapTags.
        var tableOrder = new[] { "Maps", "MapDetails", "Hints", "MapTags" };
        var orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < tableOrder.Length; i++)
            orderIndex[tableOrder[i]] = i;
        var ordered = inserts
            .OrderBy(x => orderIndex.TryGetValue(x.Table, out var idx) ? idx : int.MaxValue)
            .ThenBy(x => x.Table)
            .ThenBy(x => x.SourceOrder)
            .ToList();

        var systemUserIdStr = systemUserId.ToString("D");
        // Replace CreatedBy/UpdatedBy in script with systemUserId to avoid FK_Maps_Users_CreatedBy errors.
        var scriptUserIdLiteral = $"N'{ScriptCreatedByUserIdLiteral}'";

        logger.LogInformation("Seeding maps data from SQL script: {Path}. Statements: {Count}", scriptPath, ordered.Count);

        int executed = 0;
        int skipped = 0;

        async Task RunOneInsertAsync(InsertStatementInfo item)
        {
            var table = item.Table;
            var statement = item.Statement.Replace(scriptUserIdLiteral, $"N'{systemUserIdStr}'", StringComparison.OrdinalIgnoreCase);
            var id = item.Id;
            if (!allowedTables.Contains(table))
                return;

            if (string.Equals(table, "MapTags", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var (scriptTagId, currentTagId) in scriptTagIdToCurrentId)
                    statement = statement.Replace($"N'{scriptTagId}'", $"N'{currentTagId}'", StringComparison.OrdinalIgnoreCase);
            }

            string pgSql;
            try
            {
                pgSql = SqlServerToPostgreSqlInsertConverter.ConvertInsertStatement(statement);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to convert INSERT to PostgreSQL (Table {Table}, Id {Id}).", table, id);
                skipped++;
                return;
            }

            var statementEscaped = pgSql.Replace("{", "{{").Replace("}", "}}");
            var affected = await dbContext.Database.ExecuteSqlRawAsync(statementEscaped);
            if (affected > 0) executed++; else skipped++;
        }

        // Phase 1: seed Maps first (MapDetails/Hints/MapTags reference MapId).
        foreach (var item in ordered.Where(x => string.Equals(x.Table, "Maps", StringComparison.OrdinalIgnoreCase)))
            await RunOneInsertAsync(item);

        var mapIds = (await dbContext.Maps.AsNoTracking().Select(m => m.Id).ToListAsync()).ToHashSet();

        // Phase 2: seed MapDetails/Hints/MapTags; skip rows when parent MapId does not exist.
        foreach (var item in ordered.Where(x => !string.Equals(x.Table, "Maps", StringComparison.OrdinalIgnoreCase)))
        {
            var mapIdMatch = ChildInsertMapIdRegex.Match(item.Statement);
            if (!mapIdMatch.Success || !Guid.TryParse(mapIdMatch.Groups["mapId"].Value, out var fkMapId))
            {
                logger.LogWarning("Skip {Table} Id {RowId}: cannot parse MapId from VALUES.", item.Table, item.Id);
                skipped++;
                continue;
            }

            if (!mapIds.Contains(fkMapId))
            {
                logger.LogWarning(
                    "Skip {Table} Id {RowId}: MapId {MapId} does not exist in Maps table.",
                    item.Table,
                    item.Id,
                    fkMapId);
                skipped++;
                continue;
            }

            await RunOneInsertAsync(item);
        }

        logger.LogInformation("Seed maps from SQL script done. Executed: {Executed}, Skipped: {Skipped}", executed, skipped);
    }

    private sealed class InsertStatementInfo
    {
        public string Table { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Statement { get; set; } = string.Empty;
        /// <summary>Original statement order in script file (stable sort within each table).</summary>
        public int SourceOrder { get; set; }
    }

    /// <summary>For MapDetails/Hints/MapTags, the second VALUES column is MapId (N'guid').</summary>
    private static readonly Regex ChildInsertMapIdRegex = new(
        @"VALUES\s*\(\s*N'(?<rowId>[0-9a-fA-F-]{36})'\s*,\s*N'(?<mapId>[0-9a-fA-F-]{36})'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Read script and extract (TagId, Name) from INSERT [dbo].[Tags] to remap Tag Id in DB.</summary>
    private static List<(string TagId, string Name)> ExtractScriptTagIdToName(string scriptPath)
    {
        var list = new List<(string, string)>();
        var tagInsertRegex = new Regex(@"^INSERT\s+\[dbo\]\.\[Tags\].*?VALUES\s*\(\s*N'(?<id>[0-9a-fA-F-]{36})'\s*,\s*N'(?<name>[^']*)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        using var reader = new StreamReader(scriptPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine() ?? string.Empty;
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("INSERT [dbo].[Tags]", StringComparison.OrdinalIgnoreCase))
                continue;
            var m = tagInsertRegex.Match(trimmed);
            if (m.Success)
                list.Add((m.Groups["id"].Value, m.Groups["name"].Value));
        }
        return list;
    }

    private static IEnumerable<InsertStatementInfo> ExtractInsertStatements(string scriptPath, HashSet<string> allowedTables)
    {
        var idRegex = new Regex(@"VALUES\s*\(\s*N'(?<id>[0-9a-fA-F-]{36})'", RegexOptions.Compiled);
        var tableRegex = new Regex(@"^INSERT\s+\[dbo\]\.\[(?<table>[^\]]+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        using var reader = new StreamReader(scriptPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        var sb = new StringBuilder();
        bool capturing = false;

        static InsertStatementInfo? TryBuild(string stmt, Regex tableRegex, Regex idRegex, HashSet<string> allowedTables)
        {
            if (string.IsNullOrWhiteSpace(stmt))
                return null;

            var tableMatch = tableRegex.Match(stmt);
            var idMatch = idRegex.Match(stmt);
            if (tableMatch.Success && idMatch.Success && allowedTables.Contains(tableMatch.Groups["table"].Value))
                return new InsertStatementInfo
                {
                    Table = tableMatch.Groups["table"].Value,
                    Id = idMatch.Groups["id"].Value,
                    Statement = stmt
                };
            return null;
        }

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine() ?? string.Empty;
            var trimmed = line.Trim();

            // "GO" always terminates the current statement.
            if (string.Equals(trimmed, "GO", StringComparison.OrdinalIgnoreCase))
            {
                if (capturing)
                {
                    var item = TryBuild(sb.ToString().Trim(), tableRegex, idRegex, allowedTables);
                    if (item is not null) yield return item;
                    sb.Clear();
                }

                capturing = false;
                continue;
            }

            // Some sections in script do not separate INSERT statements with GO (especially Maps block).
            // When a new INSERT line is detected, flush previous statement and start a new one.
            if (trimmed.StartsWith("INSERT [dbo].[", StringComparison.OrdinalIgnoreCase))
            {
                if (capturing)
                {
                    var item = TryBuild(sb.ToString().Trim(), tableRegex, idRegex, allowedTables);
                    if (item is not null) yield return item;
                    sb.Clear();
                }

                var end = trimmed.IndexOf(']', "INSERT [dbo].[".Length);
                if (end > 0)
                {
                    var table = trimmed.Substring("INSERT [dbo].[".Length, end - "INSERT [dbo].[".Length);
                    if (allowedTables.Contains(table))
                    {
                        capturing = true;
                        sb.AppendLine(line);
                    }
                    else
                    {
                        capturing = false;
                        sb.Clear();
                    }
                }

                continue;
            }

            if (capturing)
                sb.AppendLine(line);
        }

        if (capturing)
        {
            var item = TryBuild(sb.ToString().Trim(), tableRegex, idRegex, allowedTables);
            if (item is not null) yield return item;
        }
    }
}





