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
                    CreatedAt = DateTime.UtcNow
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
                JoiningAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
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
                    JoiningAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
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
                CreatedAt = DateTime.UtcNow,
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
                seed.CreatedAt = DateTime.UtcNow;
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
                existing.UpdatedAt = DateTime.UtcNow;
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
                CreatedAt = DateTime.UtcNow,
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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = existingAdmin?.Id ?? Guid.Empty,
                Status = EntityStatusEnum.Active
            };
            await dbContext.Payments.AddAsync(payos);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded payment method: PayOS.");
        }

        // Seed maps từ file SQL (INSERT Maps/MapDetails/Hints/MapTags) — bật trong appsettings: DataSeeding:SeedMapsFromSqlScript
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var seedMapsFromSqlScript = configuration.GetSection("DataSeeding").GetValue<bool>("SeedMapsFromSqlScript");
        if (seedMapsFromSqlScript)
        {
            // Neon / Postgres pooler (host thường có "-pooler") vẫn chạy seed SQL được; chỉ tắt bằng DataSeeding:SeedMapsFromSqlScript=false nếu cần.
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

        // Seed Learning Goals (idempotent by Name) – lộ trình học
        var learningGoalSeeds = new[]
        {
            new { Name = "Logic cơ bản", Description = "Làm quen với biến, phép toán, thứ tự thực thi và điều khiển luồng cơ bản.", SortOrder = 1 },
            new { Name = "Điều kiện", Description = "Học cách dùng if/else, so sánh và rẽ nhánh trong chương trình.", SortOrder = 2 },
            new { Name = "Vòng lặp", Description = "Làm chủ for, while và xử lý lặp để giải quyết bài toán.", SortOrder = 3 },
            new { Name = "Giải quyết vấn đề", Description = "Kết hợp logic, điều kiện và vòng lặp để phân tích và giải bài toán.", SortOrder = 4 }
        };

        var existingGoalNames = await dbContext.LearningGoals
            .AsNoTracking()
            .Select(g => g.Name)
            .ToListAsync();
        var existingGoalSet = new HashSet<string>(existingGoalNames, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in learningGoalSeeds)
        {
            if (existingGoalSet.Contains(seed.Name))
            {
                logger.LogInformation("Learning goal already exists: {Name}", seed.Name);
                continue;
            }

            var goal = new LearningGoal
            {
                Name = seed.Name,
                Description = seed.Description,
                SortOrder = seed.SortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = existingAdmin?.Id,
                Status = EntityStatusEnum.Active
            };
            goal.InitializeEntity(existingAdmin?.Id);
            await dbContext.LearningGoals.AddAsync(goal);
            await dbContext.SaveChangesAsync();
            existingGoalSet.Add(seed.Name);
            logger.LogInformation("Seeded learning goal: {Name}", seed.Name);
        }

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

        var existingConceptKeys = await dbContext.Concepts
            .Where(c => !c.IsDeleted)
            .Select(c => new { c.LearningGoalId, c.Name })
            .ToListAsync();
        var existingConceptSet = new HashSet<(Guid, string)>(existingConceptKeys.Select(k => (k.LearningGoalId, k.Name)), new ConceptKeyComparer());

        foreach (var (goalName, name, description, contentKey, sortOrder) in conceptSeeds)
        {
            if (!goalsByName.TryGetValue(goalName, out var goalId))
                continue;
            if (existingConceptSet.Contains((goalId, name)))
            {
                logger.LogInformation("Concept already exists: {GoalName} / {Name}", goalName, name);
                continue;
            }

            var concept = new Concept
            {
                LearningGoalId = goalId,
                Name = name,
                Description = description,
                ContentKey = contentKey,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = existingAdmin?.Id,
                Status = EntityStatusEnum.Active
            };
            concept.InitializeEntity(existingAdmin?.Id);
            await dbContext.Concepts.AddAsync(concept);
            await dbContext.SaveChangesAsync();
            existingConceptSet.Add((goalId, name));
            logger.LogInformation("Seeded concept: {GoalName} / {Name}", goalName, name);
        }

        // Seed LearningPathItems (idempotent: skip row nếu (LearningGoalId, SortOrder) đã tồn tại)
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

        // Title phải khớp cột Maps.Title (vd. script_clean.sql / map đã publish). Gán map theo từng concept cho hợp lý.
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
            // Legacy (nếu DB cũ chỉ có các map cũ)
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

        // (GoalName, ItemType, ConceptName?, MapTitle?, SortOrder) — MapTitle ưu tiên map mới, fallback legacy nếu chưa seed SQL
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
            if (existingPathItemSet.Contains((goalId, sortOrder)))
            {
                logger.LogInformation("Path item already exists: {GoalName} SortOrder={SortOrder}", goalName, sortOrder);
                continue;
            }

            Guid? conceptId = null;
            Guid? mapId = null;
            if (itemType == LearningPathItemTypeEnum.Concept && !string.IsNullOrEmpty(conceptName) && conceptIdLookup.TryGetValue(goalId, out var byName) && byName.TryGetValue(conceptName, out var cId))
                conceptId = cId;
            if (itemType == LearningPathItemTypeEnum.Map && !string.IsNullOrEmpty(mapTitle) && mapIdsByTitle.TryGetValue(mapTitle, out var mId))
                mapId = mId;

            var item = new LearningPathItem
            {
                LearningGoalId = goalId,
                ItemType = itemType,
                ConceptId = conceptId,
                MapId = mapId,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = existingAdmin?.Id,
                Status = EntityStatusEnum.Active
            };
            item.InitializeEntity(existingAdmin?.Id);
            await dbContext.LearningPathItems.AddAsync(item);
            await dbContext.SaveChangesAsync();
            existingPathItemSet.Add((goalId, sortOrder));
            logger.LogInformation("Seeded path item: {GoalName} SortOrder={SortOrder} {Type}", goalName, sortOrder, itemType);
        }

        logger.LogInformation("Data seeding completed.");
    }

    private sealed class ConceptKeyComparer : IEqualityComparer<(Guid, string)>
    {
        public bool Equals((Guid, string) x, (Guid, string) y) => x.Item1 == y.Item1 && string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode((Guid, string) obj) => HashCode.Combine(obj.Item1, obj.Item2.GetHashCode(StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>GUID user trong script_clean.sql dùng cho CreatedBy/UpdatedBy. Sẽ được thay bằng systemUserId khi seed.</summary>
    private const string ScriptCreatedByUserIdLiteral = "29f8c7e0-11bb-46c1-327b-08de83cfc02d";

    private static async Task SeedMapsFromSqlScriptAsync(CapstoneProjectDbContext dbContext, string scriptPath, Guid systemUserId, ILogger logger)
    {
        if (!File.Exists(scriptPath))
        {
            logger.LogWarning("Maps SQL script not found: {Path}", scriptPath);
            return;
        }

        // Chỉ INSERT dữ liệu; không chạy DDL. Tags dùng sẵn đã seed bên ngoài (defaultTagNames); MapTags sẽ map TagId trong script sang Id tag trong DB theo Name.
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

        // Map TagId trong script -> Tag Id trong DB (theo Name). Tag trong DB đã seed trước (defaultTagNames).
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

        // Thứ tự: Maps, MapDetails, Hints, MapTags.
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
        // Thay CreatedBy/UpdatedBy trong script bằng systemUserId để tránh lỗi FK_Maps_Users_CreatedBy.
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
                logger.LogError(ex, "Không chuyển được INSERT sang PostgreSQL (bảng {Table}, Id {Id}).", table, id);
                skipped++;
                return;
            }

            var statementEscaped = pgSql.Replace("{", "{{").Replace("}", "}}");
            var affected = await dbContext.Database.ExecuteSqlRawAsync(statementEscaped);
            if (affected > 0) executed++; else skipped++;
        }

        // Pha 1: Maps trước (FK MapDetails/Hints/MapTags trỏ MapId).
        foreach (var item in ordered.Where(x => string.Equals(x.Table, "Maps", StringComparison.OrdinalIgnoreCase)))
            await RunOneInsertAsync(item);

        var mapIds = (await dbContext.Maps.AsNoTracking().Select(m => m.Id).ToListAsync()).ToHashSet();

        // Pha 2: MapDetails, Hints, MapTags — bỏ qua nếu MapId chưa có (script thiếu map cha hoặc insert map lỗi trước đó).
        foreach (var item in ordered.Where(x => !string.Equals(x.Table, "Maps", StringComparison.OrdinalIgnoreCase)))
        {
            var mapIdMatch = ChildInsertMapIdRegex.Match(item.Statement);
            if (!mapIdMatch.Success || !Guid.TryParse(mapIdMatch.Groups["mapId"].Value, out var fkMapId))
            {
                logger.LogWarning("Bỏ qua {Table} Id {RowId}: không đọc được MapId từ VALUES.", item.Table, item.Id);
                skipped++;
                continue;
            }

            if (!mapIds.Contains(fkMapId))
            {
                logger.LogWarning(
                    "Bỏ qua {Table} Id {RowId}: MapId {MapId} không tồn tại trong bảng Maps (thiếu INSERT map hoặc map chưa vào DB).",
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
        /// <summary>Thứ tự xuất hiện trong file script (ổn định khi sort theo bảng).</summary>
        public int SourceOrder { get; set; }
    }

    /// <summary>MapDetails / Hints / MapTags: cột thứ 2 sau VALUES là MapId (N'guid').</summary>
    private static readonly Regex ChildInsertMapIdRegex = new(
        @"VALUES\s*\(\s*N'(?<rowId>[0-9a-fA-F-]{36})'\s*,\s*N'(?<mapId>[0-9a-fA-F-]{36})'",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Đọc script, lấy (TagId, Name) từ INSERT [dbo].[Tags] để map sang Tag Id trong DB.</summary>
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

            // "GO" luôn kết thúc statement hiện tại (nếu có).
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

            // Nhiều đoạn trong script không có GO giữa các INSERT (đặc biệt block Maps).
            // Vì vậy khi gặp 1 dòng INSERT mới, flush statement trước đó và bắt đầu statement mới.
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


