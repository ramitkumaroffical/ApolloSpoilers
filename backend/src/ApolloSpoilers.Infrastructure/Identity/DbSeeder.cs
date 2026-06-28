using ApolloSpoilers.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApolloSpoilers.Infrastructure.Identity;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, roleManager);
        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.Roles.AnyAsync(r => r.Name == role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        var email = "admin@apollospoilers.com";
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            FirstName = "Admin",
            LastName = "Apollo",
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin#123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new() { Name = "Rear Spoilers", Slug = "rear-spoilers", Description = "Rear lip and wing spoilers for enhanced downforce and style." },
            new() { Name = "Front Splitters", Slug = "front-splitters", Description = "Front splitter lip and canards for aggressive front-end styling." },
            new() { Name = "GT Wings", Slug = "gt-wings", Description = "High-performance adjustable GT-style wings for track and show." },
            new() { Name = "Lip Spoilers", Slug = "lip-spoilers", Description = "Subtle OEM+ lip spoilers for a refined look." },
            new() { Name = "Universal Accessories", Slug = "universal-accessories", Description = "Universal-fit styling accessories, diffusers, and extensions." },
            new() { Name = "Roof Spoilers", Slug = "roof-spoilers", Description = "Roof-mounted spoilers and shark-fin antenna spoilers." },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var categories = await context.Categories.ToListAsync();
        var rearSpoiler = categories.First(c => c.Slug == "rear-spoilers");
        var gtWing = categories.First(c => c.Slug == "gt-wings");
        var lipSpoiler = categories.First(c => c.Slug == "lip-spoilers");
        var frontSplitter = categories.First(c => c.Slug == "front-splitters");
        var universal = categories.First(c => c.Slug == "universal-accessories");
        var roofSpoiler = categories.First(c => c.Slug == "roof-spoilers");

        var products = new List<Product>
        {
            // Rear Spoilers
            new() { Name = "AP Performance GT250 Rear Wing", Slug = "ap-performance-gt250-rear-wing", Description = "High-quality FRP rear wing inspired by the Nismo GT250. Direct bolt-on with adjustable angle. Delivers real downforce at speed.", Price = 389.99m, CompareAtPrice = 449.99m, Material = "FRP (Fiberglass)", Color = "Unpainted Black Primer", CarBrand = "Nissan", CarModel = "GT-R R35", FitYearFrom = 2009, FitYearTo = 2024, CategoryId = rearSpoiler.Id, IsFeatured = true, Inventory = new Inventory { StockQuantity = 25, LowStockThreshold = 5 } },
            new() { Name = "Evo X Twister Rear Spoiler", Slug = "evo-x-twister-rear-spoiler", Description = "Aggressive twister-style rear spoiler for the Lancer Evolution X. Lightweight ABS plastic construction.", Price = 275.00m, Material = "ABS Plastic", Color = "Textured Black", CarBrand = "Mitsubishi", CarModel = "Lancer Evolution X", FitYearFrom = 2008, FitYearTo = 2016, CategoryId = rearSpoiler.Id, IsFeatured = true, Inventory = new Inventory { StockQuantity = 15, LowStockThreshold = 3 } },
            new() { Name = "Honda Civic Type R Wing", Slug = "honda-civic-type-r-wing", Description = "OEM-style high-mount rear wing for FK7/FK8 Civic Type R. Carbon fiber top plate.", Price = 549.99m, Material = "Carbon Fiber", Color = "Gloss Carbon", CarBrand = "Honda", CarModel = "Civic Type R", FitYearFrom = 2017, FitYearTo = 2025, CategoryId = rearSpoiler.Id, IsFeatured = true, Inventory = new Inventory { StockQuantity = 10, LowStockThreshold = 2 } },
            new() { Name = "Toyota Supra MK5 Ducktail Spoiler", Slug = "toyota-supra-mk5-ducktail", Description = "Elegant ducktail spoiler for the A90 Supra. Enhances the rear silhouette without being over the top.", Price = 225.00m, Material = "PU (Polyurethane)", Color = "Unpainted", CarBrand = "Toyota", CarModel = "Supra A90", FitYearFrom = 2020, FitYearTo = 2025, CategoryId = rearSpoiler.Id, Inventory = new Inventory { StockQuantity = 20, LowStockThreshold = 5 } },

            // GT Wings
            new() { Name = "Universal 67-inch GT Wing", Slug = "universal-67-inch-gt-wing", Description = "Universal 67-inch adjustable GT wing with 3-position blade. Aluminum uprights. Fits most vehicles with trunk mount.", Price = 499.99m, Material = "Aluminum + FRP", Color = "Matte Black", CategoryId = gtWing.Id, IsFeatured = true, Inventory = new Inventory { StockQuantity = 30, LowStockThreshold = 5 } },
            new() { Name = "Drift-spec 1300mm GT Wing", Slug = "drift-spec-1300mm-gt-wing", Description = "Wide 1300mm blade GT wing designed for drift and time attack. Swivel mount for angle adjustment.", Price = 425.00m, Material = "Carbon Fiber Blade / Aluminum Stands", CategoryId = gtWing.Id, Inventory = new Inventory { StockQuantity = 12, LowStockThreshold = 3 } },

            // Lip Spoilers
            new() { Name = "BMW M3/M4 G80/G82 Lip Spoiler", Slug = "bmw-m3-m4-lip-spoiler", Description = "Subtle OEM+ lip spoiler for G80 M3 and G82 M4. Adhesive-bonded installation.", Price = 149.99m, Material = "Real Carbon Fiber", Color = "Gloss Carbon", CarBrand = "BMW", CarModel = "M3 / M4 G80/G82", FitYearFrom = 2021, FitYearTo = 2025, CategoryId = lipSpoiler.Id, Inventory = new Inventory { StockQuantity = 35, LowStockThreshold = 8 } },
            new() { Name = "Subaru WRX/STI Trunk Lip", Slug = "subaru-wrx-sti-trunk-lip", Description = "Clean trunk lip for VA/VB WRX and STI. ABS construction, paintable.", Price = 119.00m, Material = "ABS Plastic", Color = "Unpainted", CarBrand = "Subaru", CarModel = "WRX / STI", FitYearFrom = 2015, FitYearTo = 2025, CategoryId = lipSpoiler.Id, Inventory = new Inventory { StockQuantity = 22, LowStockThreshold = 5 } },

            // Front Splitters
            new() { Name = "Mustang GT Front Lip Splitter", Slug = "mustang-gt-front-lip-splitter", Description = "Aggressive front chin splitter for S550 Mustang GT. ABS plastic with matte black finish.", Price = 189.99m, Material = "ABS Plastic", Color = "Matte Black", CarBrand = "Ford", CarModel = "Mustang GT S550", FitYearFrom = 2015, FitYearTo = 2023, CategoryId = frontSplitter.Id, Inventory = new Inventory { StockQuantity = 18, LowStockThreshold = 4 } },
            new() { Name = "Supra A90 Front Canard Set", Slug = "supra-a90-front-canard-set", Description = "Pair of front canards for the A90 Supra. Generates additional front downforce.", Price = 139.00m, Material = "Carbon Fiber", Color = "Gloss Carbon", CarBrand = "Toyota", CarModel = "Supra A90", FitYearFrom = 2020, FitYearTo = 2025, CategoryId = frontSplitter.Id, Inventory = new Inventory { StockQuantity = 14, LowStockThreshold = 3 } },

            // Universal
            new() { Name = "Universal Rear Diffuser 4-Pipe", Slug = "universal-rear-diffuser-4-pipe", Description = "Universal fit rear diffuser with 4 exhaust cutouts. Mounts to most vehicles with modification.", Price = 199.99m, Material = "FRP", Color = "Unpainted Black Primer", CategoryId = universal.Id, Inventory = new Inventory { StockQuantity = 25, LowStockThreshold = 5 } },
            new() { Name = "Universal Fender Flare Set (4pc)", Slug = "universal-fender-flare-set", Description = "Universal bolt-on fender flares, set of 4. Adds up to 25mm width per side.", Price = 299.00m, Material = "ABS Plastic", Color = "Textured Black", CategoryId = universal.Id, Inventory = new Inventory { StockQuantity = 20, LowStockThreshold = 5 } },
            new() { Name = "Side Skirt Extension Kit", Slug = "universal-side-skirt-extension", Description = "Universal side skirt extensions that give a lowered, wider appearance. Self-adhesive + screw mount.", Price = 159.99m, Material = "PU (Polyurethane)", Color = "Unpainted", CategoryId = universal.Id, Inventory = new Inventory { StockQuantity = 28, LowStockThreshold = 6 } },

            // Roof
            new() { Name = "Universal Shark Fin Antenna", Slug = "universal-shark-fin-antenna", Description = "Shark-fin style antenna cover. Replaces bulky factory antennas. Universal fit.", Price = 29.99m, Material = "ABS Plastic", Color = "Gloss Black", CategoryId = roofSpoiler.Id, Inventory = new Inventory { StockQuantity = 100, LowStockThreshold = 20 } },
            new() { Name = "Honda Civic Roof Spoiler FK7", Slug = "honda-civic-roof-spoiler-fk7", Description = "Roof-mount lip spoiler for 10th gen Civic sedan FK7.", Price = 179.00m, Material = "FRP", Color = "Unpainted", CarBrand = "Honda", CarModel = "Civic FK7", FitYearFrom = 2016, FitYearTo = 2021, CategoryId = roofSpoiler.Id, Inventory = new Inventory { StockQuantity = 12, LowStockThreshold = 3 } },
        };

        // Add placeholder images to each product
        foreach (var product in products)
        {
            product.Images.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}-1.jpg",
                IsPrimary = true,
                DisplayOrder = 1
            });
            product.Images.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}-2.jpg",
                IsPrimary = false,
                DisplayOrder = 2
            });
        }

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
