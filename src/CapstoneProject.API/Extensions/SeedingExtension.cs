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

        // Seed 2 moderator accounts for moderation workflows
        var moderatorRoleName = RoleEnum.Moderator.ToString();
        var moderatorSeeds = new[]
        {
            new { Email = "moderator1@capstoneproject.com", Password = "Moderator@123", FirstName = "Content", LastName = "Moderator1" },
            new { Email = "moderator2@capstoneproject.com", Password = "Moderator@123", FirstName = "Content", LastName = "Moderator2" }
        };

        foreach (var seed in moderatorSeeds)
        {
            var existingModerator = await userManager.FindByEmailAsync(seed.Email);
            if (existingModerator == null)
            {
                var moderatorUser = new AppUser
                {
                    UserName = seed.Email,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    FirstName = seed.FirstName,
                    LastName = seed.LastName,
                    JoiningAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                    Status = EntityStatusEnum.Active
                };

                var createModeratorResult = await userManager.CreateAsync(moderatorUser, seed.Password);
                if (!createModeratorResult.Succeeded)
                {
                    var errors = string.Join(", ", createModeratorResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create moderator user {Email}: {Errors}", seed.Email, errors);
                    continue;
                }

                existingModerator = moderatorUser;
                logger.LogInformation("Created moderator user {Email}", seed.Email);
            }

            if (!await userManager.IsInRoleAsync(existingModerator, moderatorRoleName))
            {
                var addModeratorRoleResult = await userManager.AddToRoleAsync(existingModerator, moderatorRoleName);
                if (!addModeratorRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addModeratorRoleResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to add moderator user {Email} to role {Role}: {Errors}", seed.Email, moderatorRoleName, errors);
                }
                else
                {
                    logger.LogInformation("Added moderator user {Email} to role {Role}", seed.Email, moderatorRoleName);
                }
            }
        }

        // Seed game tags (idempotent)
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
                FeaturesSpec = "Play basic games; max 20 games; no hints; cannot create/publish games; no XP boost."
            },
            new Package
            {
                Name = "Pro",
                DurationDays = 30,
                Limit = null,
                Price = 149m,
                FeaturesSpec = "Play basic and advanced games; hints enabled; cannot create/publish games; XP boost enabled."
            },
            new Package
            {
                Name = "Creator",
                DurationDays = 30,
                Limit = null,
                Price = 299m,
                FeaturesSpec = "Play basic and advanced games; hints enabled; can create and publish games; game analytics; XP boost enabled."
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

        // Seed games from SQL file (INSERT Games/GameDetails/Hints/GameTags), toggled by DataSeeding:SeedMapsFromSqlScript.
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
            logger.LogInformation("Game seeding from SQL script is disabled (DataSeeding:SeedMapsFromSqlScript=false).");
        }

        await BackfillMapVersionLineDataAsync(dbContext, existingAdmin?.Id, logger);

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

        // Title must match Games.Title (for example from script_clean.sql / published games). Game assignment follows concept flow.
        var mapTitles = new[]
        {
            "Introduce variable",
            "Mathematical operation",
            "Platform movement tutorial",
            "Introduce trap",
            "More Box",
            "Introduce for loop",
            "Introduce while/do while loop",
            "Basic top down game",
            "Maze game",
            // Legacy fallbacks (if old DB only has legacy game titles).
            "level-platform-01",
            "level-topdown-1771989668367",
            "level-topdown-foreground-example"
        };
        var mapsInList = await dbContext.Games
            .Where(m => !m.IsDeleted && mapTitles.Contains(m.Title))
            .Select(m => new { m.Title, m.Id })
            .ToListAsync();
        var gameIdsByTitle = mapsInList
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

        // (GoalName, ItemType, ConceptName?, MapTitle?, SortOrder) with preferred new game title and legacy fallback.
        var pathItemSeeds = new[]
        {
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Biến là gì", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce variable", "level-platform-01", "level-topdown-1771989668367", gameIdsByTitle), SortOrder: 2),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Phép toán", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Mathematical operation", "level-topdown-1771989668367", "level-platform-01", gameIdsByTitle), SortOrder: 4),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Thứ tự thực thi", MapTitle: (string?)null, SortOrder: 5),
            (GoalName: "Logic cơ bản", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Platform movement tutorial", "level-topdown-foreground-example", "level-platform-01", gameIdsByTitle), SortOrder: 6),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "If-else", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce trap", "level-platform-01", "level-topdown-1771989668367", gameIdsByTitle), SortOrder: 2),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "So sánh", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Điều kiện", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("More Box", "level-topdown-1771989668367", "level-platform-01", gameIdsByTitle), SortOrder: 4),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "For loop", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce for loop", "level-platform-01", "level-topdown-1771989668367", gameIdsByTitle), SortOrder: 2),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "While loop", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Vòng lặp", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Introduce while/do while loop", "level-topdown-1771989668367", "level-platform-01", gameIdsByTitle), SortOrder: 4),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Phân tích bài toán", MapTitle: (string?)null, SortOrder: 1),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Basic top down game", "level-topdown-1771989668367", "level-platform-01", gameIdsByTitle), SortOrder: 2),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Concept, ConceptName: "Thuật toán cơ bản", MapTitle: (string?)null, SortOrder: 3),
            (GoalName: "Giải quyết vấn đề", ItemType: LearningPathItemTypeEnum.Game, ConceptName: (string?)null, MapTitle: PickMapTitle("Maze game", "level-platform-01", "level-topdown-foreground-example", gameIdsByTitle), SortOrder: 4)
        };

        foreach (var (goalName, itemType, conceptName, mapTitle, sortOrder) in pathItemSeeds)
        {
            if (!goalsByName.TryGetValue(goalName, out var goalId))
                continue;
            Guid? conceptId = null;
            Guid? gameId = null;
            if (itemType == LearningPathItemTypeEnum.Concept && !string.IsNullOrEmpty(conceptName) && conceptIdLookup.TryGetValue(goalId, out var byName) && byName.TryGetValue(conceptName, out var cId))
                conceptId = cId;
            if (itemType == LearningPathItemTypeEnum.Game && !string.IsNullOrEmpty(mapTitle) && gameIdsByTitle.TryGetValue(mapTitle, out var mId))
                gameId = mId;

            var existingPathItem = await dbContext.LearningPathItems
                .FirstOrDefaultAsync(i => !i.IsDeleted && i.LearningGoalId == goalId && i.SortOrder == sortOrder);

            if (existingPathItem == null)
            {
                var item = new LearningPathItem
                {
                    LearningGoalId = goalId,
                    ItemType = itemType,
                    ConceptId = conceptId,
                    GameId = gameId,
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
                // Do not erase an existing GameId when current title lookup fails.
                if (itemType == LearningPathItemTypeEnum.Game && gameId.HasValue)
                    existingPathItem.GameId = gameId;
                else if (itemType == LearningPathItemTypeEnum.Concept)
                    existingPathItem.GameId = null;

                existingPathItem.Status = EntityStatusEnum.Active;
                existingPathItem.UpdateEntity(existingAdmin?.Id);
                logger.LogInformation("Updated path item: {GoalName} SortOrder={SortOrder} {Type}", goalName, sortOrder, itemType);
            }
        }
        await dbContext.SaveChangesAsync();

        await SeedXpConfigurationDataAsync(dbContext, existingAdmin?.Id, logger);
        await SeedComplaintConfigurationDataAsync(dbContext, existingAdmin?.Id, logger);

        logger.LogInformation("Data seeding completed.");
    }

    private static async Task BackfillMapVersionLineDataAsync(CapstoneProjectDbContext dbContext, Guid? userId, ILogger logger)
    {
        var actorId = userId ?? Guid.Empty;

        // Legacy games created before version-line feature: game is root of its own line.
        var mapsMissingRoot = await dbContext.Games
            .Where(m => !m.IsDeleted && m.RootGameId == null)
            .ToListAsync();

        if (mapsMissingRoot.Count > 0)
        {
            foreach (var game in mapsMissingRoot)
            {
                game.RootGameId = game.Id;
                game.IsActiveVersion = true;
                game.UpdateEntity(actorId);
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation("Backfilled RootGameId for {Count} legacy games.", mapsMissingRoot.Count);
        }

        // Normalize: keep only one active version per line.
        var rootGameIds = await dbContext.Games
            .Where(m => !m.IsDeleted)
            .Select(m => m.RootGameId ?? m.Id)
            .Distinct()
            .ToListAsync();

        var normalizedLines = 0;
        foreach (var rootGameId in rootGameIds)
        {
            var lineMaps = await dbContext.Games
                .Where(m => !m.IsDeleted && (m.RootGameId ?? m.Id) == rootGameId)
                .OrderByDescending(m => m.IsPublished)
                .ThenByDescending(m => m.ContentVersion)
                .ThenByDescending(m => m.CreatedAt)
                .ToListAsync();

            if (lineMaps.Count == 0)
                continue;

            var shouldBeActive = lineMaps.First();
            var changed = false;

            foreach (var game in lineMaps)
            {
                var expectedActive = game.Id == shouldBeActive.Id;
                if (game.IsActiveVersion != expectedActive)
                {
                    game.IsActiveVersion = expectedActive;
                    game.UpdateEntity(actorId);
                    changed = true;
                }
            }

            if (changed)
                normalizedLines++;
        }

        if (normalizedLines > 0)
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Normalized active-version flag for {Count} game lines.", normalizedLines);
        }
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

        var mapSolveExisting = await dbContext.GameSolveScoreConfigs.FirstOrDefaultAsync(
            x => !x.IsDeleted && x.ConfigKey == GameSolveScoreConfig.DefaultConfigKey);
        if (mapSolveExisting == null)
        {
            var mapSolveRow = new GameSolveScoreConfig
            {
                ConfigKey = GameSolveScoreConfig.DefaultConfigKey,
                BaseScore = 10,
                TimeScore = 30,
                StepsScore = 30,
                BlocksScore = 30,
                Status = EntityStatusEnum.Active
            };
            mapSolveRow.InitializeEntity(actorId);
            await dbContext.GameSolveScoreConfigs.AddAsync(mapSolveRow);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("XP configuration seeding completed.");
    }

    private static async Task SeedComplaintConfigurationDataAsync(CapstoneProjectDbContext dbContext, Guid? userId, ILogger logger)
    {
        var actorId = userId ?? Guid.Empty;

        var categorySeeds = new[]
        {
            new { Key = "PaymentIssue", Name = "Payment Issue", Description = "Payment succeeded but entitlement/balance is incorrect", Sort = 10 },
            new { Key = "AccessIssue", Name = "Access Issue", Description = "Purchased game/package but cannot access", Sort = 20 },
            new { Key = "GameplayScoringIssue", Name = "Gameplay Scoring Issue", Description = "Unexpected score/stars/status after playing", Sort = 30 },
            new { Key = "RewardBalanceIssue", Name = "Reward Balance Issue", Description = "XP or OrbitCoin balance mismatch", Sort = 40 },
            new { Key = "TrialIssue", Name = "Trial Issue", Description = "Free trial attempts blocked or deducted incorrectly", Sort = 50 },
            new { Key = "Other", Name = "Other", Description = "General issue report that does not match predefined categories", Sort = 60 }
        };

        foreach (var seed in categorySeeds)
        {
            var existing = await dbContext.ComplaintCategoryCatalogs
                .FirstOrDefaultAsync(x => x.CategoryKey == seed.Key);

            if (existing == null)
            {
                var row = new ComplaintCategoryCatalog
                {
                    CategoryKey = seed.Key,
                    DisplayName = seed.Name,
                    Description = seed.Description,
                    IsEnabled = true,
                    SortOrder = seed.Sort,
                    Status = EntityStatusEnum.Active
                };
                row.InitializeEntity(actorId);
                await dbContext.ComplaintCategoryCatalogs.AddAsync(row);
            }
            else
            {
                existing.DisplayName = seed.Name;
                existing.Description = seed.Description;
                existing.IsEnabled = true;
                existing.SortOrder = seed.Sort;
                if (existing.IsDeleted)
                    existing.RestoreEntity(actorId);
                existing.UpdateEntity(actorId);
            }
        }

        var ruleSeeds = new[]
        {
            new { Category = "PaymentIssue", Rule = "required_context", Enabled = true, Priority = 10, Config = "{\"anyOf\":[\"paymentRecordId\",\"gameId\",\"packageId\"]}" },
            new { Category = "PaymentIssue", Rule = "time_window", Enabled = true, Priority = 20, Config = "{\"hours\":168}" },
            new { Category = "PaymentIssue", Rule = "duplicate_window", Enabled = true, Priority = 30, Config = "{\"hours\":72}" },
            new { Category = "PaymentIssue", Rule = "rate_limit", Enabled = true, Priority = 40, Config = "{\"maxPerDay\":3}" },

            new { Category = "AccessIssue", Rule = "required_context", Enabled = true, Priority = 10, Config = "{\"anyOf\":[\"gameId\",\"packageId\"]}" },
            new { Category = "AccessIssue", Rule = "time_window", Enabled = true, Priority = 20, Config = "{\"hours\":168}" },
            new { Category = "AccessIssue", Rule = "duplicate_window", Enabled = true, Priority = 30, Config = "{\"hours\":72}" },
            new { Category = "AccessIssue", Rule = "rate_limit", Enabled = true, Priority = 40, Config = "{\"maxPerDay\":3}" },

            new { Category = "GameplayScoringIssue", Rule = "required_context", Enabled = true, Priority = 10, Config = "{\"anyOf\":[\"submissionId\",\"playHistoryId\"]}" },
            new { Category = "GameplayScoringIssue", Rule = "time_window", Enabled = true, Priority = 20, Config = "{\"hours\":72}" },
            new { Category = "GameplayScoringIssue", Rule = "duplicate_window", Enabled = true, Priority = 30, Config = "{\"hours\":72}" },
            new { Category = "GameplayScoringIssue", Rule = "rate_limit", Enabled = true, Priority = 40, Config = "{\"maxPerDay\":3}" },

            new { Category = "RewardBalanceIssue", Rule = "required_context", Enabled = true, Priority = 10, Config = "{\"anyOf\":[\"xpTransactionId\",\"orbitCoinTransactionId\",\"submissionId\",\"gameId\"]}" },
            new { Category = "RewardBalanceIssue", Rule = "time_window", Enabled = true, Priority = 20, Config = "{\"hours\":72}" },
            new { Category = "RewardBalanceIssue", Rule = "duplicate_window", Enabled = true, Priority = 30, Config = "{\"hours\":72}" },
            new { Category = "RewardBalanceIssue", Rule = "rate_limit", Enabled = true, Priority = 40, Config = "{\"maxPerDay\":3}" },

            new { Category = "TrialIssue", Rule = "required_context", Enabled = true, Priority = 10, Config = "{\"anyOf\":[\"gameId\",\"playHistoryId\"]}" },
            new { Category = "TrialIssue", Rule = "time_window", Enabled = true, Priority = 20, Config = "{\"hours\":24}" },
            new { Category = "TrialIssue", Rule = "duplicate_window", Enabled = true, Priority = 30, Config = "{\"hours\":72}" },
            new { Category = "TrialIssue", Rule = "rate_limit", Enabled = true, Priority = 40, Config = "{\"maxPerDay\":3}" },

            new { Category = "Other", Rule = "rate_limit", Enabled = true, Priority = 40, Config = "{\"maxPerDay\":2}" }
        };

        foreach (var seed in ruleSeeds)
        {
            var existing = await dbContext.ComplaintPolicyRuleConfigs
                .FirstOrDefaultAsync(x => x.CategoryKey == seed.Category && x.RuleKey == seed.Rule);

            if (existing == null)
            {
                var row = new ComplaintPolicyRuleConfig
                {
                    CategoryKey = seed.Category,
                    RuleKey = seed.Rule,
                    IsEnabled = seed.Enabled,
                    Priority = seed.Priority,
                    ConfigJson = seed.Config,
                    Status = EntityStatusEnum.Active
                };
                row.InitializeEntity(actorId);
                await dbContext.ComplaintPolicyRuleConfigs.AddAsync(row);
            }
            else
            {
                existing.IsEnabled = seed.Enabled;
                existing.Priority = seed.Priority;
                existing.ConfigJson = seed.Config;
                if (existing.IsDeleted)
                    existing.RestoreEntity(actorId);
                existing.UpdateEntity(actorId);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Complaint configuration seeding completed.");
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
            logger.LogWarning("Games SQL script not found: {Path}", scriptPath);
            return;
        }

        // INSERT data only; no DDL. Existing seeded Tags are reused and GameTags are remapped by tag Name.
        var allowedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Games",
            "GameDetails",
            "Hints",
            "GameTags"
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

        // Game TagId from script to current Tag Id in DB by Name.
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

        // Execution order: Games, GameDetails (mỗi game có thể nhiều level — cột LevelOrder trong DB sau migration), Hints, GameTags.
        var tableOrder = new[] { "Games", "GameDetails", "Hints", "GameTags" };
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

        logger.LogInformation("Seeding games data from SQL script: {Path}. Statements: {Count}", scriptPath, ordered.Count);

        int executed = 0;
        int skipped = 0;
        var mapLimitsFromScript = new Dictionary<Guid, (int TimeLimitMs, int WinCondition)>();
        var mapTypesFromScript = new Dictionary<Guid, int>();

        async Task RunOneInsertAsync(InsertStatementInfo item)
        {
            var table = item.Table;
            var statement = item.Statement.Replace(scriptUserIdLiteral, $"N'{systemUserIdStr}'", StringComparison.OrdinalIgnoreCase);
            var id = item.Id;
            if (!allowedTables.Contains(table))
                return;

            if (string.Equals(table, "GameTags", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var (scriptTagId, currentTagId) in scriptTagIdToCurrentId)
                    statement = statement.Replace($"N'{scriptTagId}'", $"N'{currentTagId}'", StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(table, "Games", StringComparison.OrdinalIgnoreCase))
            {
                statement = SqlServerToPostgreSqlInsertConverter.PrepareMapsInsertForPostgres(
                    statement,
                    out var limits,
                    out var types);
                foreach (var kv in limits)
                    mapLimitsFromScript[kv.Key] = kv.Value;
                foreach (var kv in types)
                    mapTypesFromScript[kv.Key] = kv.Value;
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

        // Phase 1: Games only.
        foreach (var item in ordered.Where(x => string.Equals(x.Table, "Games", StringComparison.OrdinalIgnoreCase)))
            await RunOneInsertAsync(item);

        var gameIds = (await dbContext.Games.AsNoTracking().Select(m => m.Id).ToListAsync()).ToHashSet();

        // Phase 2a: GameDetails (FK GameId → Games). Hints giờ FK GameDetailId — xử lý sau khi có GameDetails.
        foreach (var item in ordered.Where(x => string.Equals(x.Table, "GameDetails", StringComparison.OrdinalIgnoreCase)))
        {
            var gameIdMatch = ChildInsertGameIdRegex.Match(item.Statement);
            if (!gameIdMatch.Success || !Guid.TryParse(gameIdMatch.Groups["gameId"].Value, out var fkGameId))
            {
                logger.LogWarning("Skip GameDetails Id {RowId}: cannot parse GameId from VALUES.", item.Id);
                skipped++;
                continue;
            }

            if (!gameIds.Contains(fkGameId))
            {
                logger.LogWarning(
                    "Skip GameDetails Id {RowId}: GameId {GameId} does not exist in Games table.",
                    item.Id,
                    fkGameId);
                skipped++;
                continue;
            }

            await RunOneInsertAsync(item);
        }

        // TimeLimitMs / WinCondition / Type đã chuyển sang GameDetails — backfill từ INSERT Games (script cũ).
        var gameIdsForBackfill = mapLimitsFromScript.Keys.Union(mapTypesFromScript.Keys).ToHashSet();
        foreach (var gameId in gameIdsForBackfill)
        {
            var hasL = mapLimitsFromScript.TryGetValue(gameId, out var lim);
            var hasT = mapTypesFromScript.TryGetValue(gameId, out var typ);
            int affected;
            if (hasL && hasT)
            {
                affected = await dbContext.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""GameDetails"" SET ""TimeLimitMs"" = {0}, ""WinCondition"" = {1}, ""Type"" = {2} WHERE ""GameId"" = {3} AND ""IsDeleted"" = false",
                    lim.TimeLimitMs,
                    lim.WinCondition,
                    typ,
                    gameId);
            }
            else if (hasL)
            {
                affected = await dbContext.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""GameDetails"" SET ""TimeLimitMs"" = {0}, ""WinCondition"" = {1} WHERE ""GameId"" = {2} AND ""IsDeleted"" = false",
                    lim.TimeLimitMs,
                    lim.WinCondition,
                    gameId);
            }
            else
            {
                affected = await dbContext.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""GameDetails"" SET ""Type"" = {0} WHERE ""GameId"" = {1} AND ""IsDeleted"" = false",
                    typ,
                    gameId);
            }

            if (affected > 0)
                logger.LogDebug("Backfilled GameDetails from legacy Games INSERT for GameId {GameId} ({Rows} row(s)).", gameId, affected);
        }

        // GameId → GameDetailId đầu tiên (LevelOrder nhỏ nhất) — script cũ gán hint theo GameId.
        var detailRows = await dbContext.GameDetails.AsNoTracking()
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.GameId).ThenBy(d => d.LevelOrder)
            .Select(d => new { d.GameId, d.Id })
            .ToListAsync();
        var gameIdToFirstDetailId = new Dictionary<Guid, Guid>();
        var validGameDetailIds = new HashSet<Guid>();
        foreach (var row in detailRows)
        {
            validGameDetailIds.Add(row.Id);
            if (!gameIdToFirstDetailId.ContainsKey(row.GameId))
                gameIdToFirstDetailId[row.GameId] = row.Id;
        }

        // Phase 2b: Hints — script cũ dùng [GameId] → đổi thành [GameDetailId] + FK đúng GameDetail.
        foreach (var item in ordered.Where(x => string.Equals(x.Table, "Hints", StringComparison.OrdinalIgnoreCase)))
        {
            var stmt = TransformHintsInsertGameIdToGameDetailId(item.Statement, gameIdToFirstDetailId, logger, item.Id);
            if (stmt == null)
            {
                skipped++;
                continue;
            }

            var gameIdMatch = ChildInsertGameIdRegex.Match(stmt);
            if (!gameIdMatch.Success || !Guid.TryParse(gameIdMatch.Groups["gameId"].Value, out var fkDetailId))
            {
                logger.LogWarning("Skip Hints Id {RowId}: cannot parse GameDetailId from VALUES after transform.", item.Id);
                skipped++;
                continue;
            }

            if (!validGameDetailIds.Contains(fkDetailId))
            {
                logger.LogWarning(
                    "Skip Hints Id {RowId}: GameDetailId {GameDetailId} does not exist.",
                    item.Id,
                    fkDetailId);
                skipped++;
                continue;
            }

            item.Statement = stmt;
            await RunOneInsertAsync(item);
        }

        // Phase 2c: GameTags (FK GameId).
        foreach (var item in ordered.Where(x => string.Equals(x.Table, "GameTags", StringComparison.OrdinalIgnoreCase)))
        {
            var gameIdMatch = ChildInsertGameIdRegex.Match(item.Statement);
            if (!gameIdMatch.Success || !Guid.TryParse(gameIdMatch.Groups["gameId"].Value, out var fkGameId))
            {
                logger.LogWarning("Skip GameTags Id {RowId}: cannot parse GameId from VALUES.", item.Id);
                skipped++;
                continue;
            }

            if (!gameIds.Contains(fkGameId))
            {
                logger.LogWarning(
                    "Skip GameTags Id {RowId}: GameId {GameId} does not exist in Games table.",
                    item.Id,
                    fkGameId);
                skipped++;
                continue;
            }

            await RunOneInsertAsync(item);
        }

        logger.LogInformation("Seed games from SQL script done. Executed: {Executed}, Skipped: {Skipped}", executed, skipped);
    }

    private sealed class InsertStatementInfo
    {
        public string Table { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Statement { get; set; } = string.Empty;
        /// <summary>Original statement order in script file (stable sort within each table).</summary>
        public int SourceOrder { get; set; }
    }

    /// <summary>GameDetails/GameTags: cột 2 là GameId. Hints (sau transform): cột 2 là GameDetailId — cùng pattern N'guid'.</summary>
    private static readonly Regex ChildInsertGameIdRegex = new(
        @"VALUES\s*\(\s*N'(?<rowId>[0-9a-fA-F-]{36})'\s*,\s*N'(?<gameId>[0-9a-fA-F-]{36})'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Script SSMS cũ: <c>INSERT Hints (... [GameId] ...)</c> → schema mới dùng <c>GameDetailId</c> (level đầu của game).</summary>
    private static string? TransformHintsInsertGameIdToGameDetailId(
        string statement,
        IReadOnlyDictionary<Guid, Guid> gameIdToFirstDetailId,
        ILogger logger,
        string rowId)
    {
        if (!statement.Contains("[GameId]", StringComparison.OrdinalIgnoreCase))
            return statement;

        var m = ChildInsertGameIdRegex.Match(statement);
        if (!m.Success || !Guid.TryParse(m.Groups["gameId"].Value, out var fkGameId))
        {
            logger.LogWarning("Hints Id {RowId}: cannot parse GameId from VALUES.", rowId);
            return null;
        }

        if (!gameIdToFirstDetailId.TryGetValue(fkGameId, out var detailId))
        {
            logger.LogWarning("Hints Id {RowId}: no GameDetail for GameId {GameId}.", rowId, fkGameId);
            return null;
        }

        var s = statement.Replace("[GameId]", "[GameDetailId]", StringComparison.OrdinalIgnoreCase);
        var mg = ChildInsertGameIdRegex.Match(s);
        if (!mg.Success)
            return null;

        var g = mg.Groups["gameId"];
        var start = g.Index - 2;
        if (start < 0 || start + g.Length + 3 > s.Length)
            return null;
        if (!s.AsSpan(start, 2).Equals("N'", StringComparison.OrdinalIgnoreCase))
            return null;

        var newN = $"N'{detailId:D}'";
        return s.Remove(start, g.Length + 3).Insert(start, newN);
    }

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

            // Some sections in script do not separate INSERT statements with GO (especially Games block).
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





