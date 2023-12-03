using ECommerce.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DataAccess.Data
{
    // On Db change:
    // add-migration changeName
    // update-database
    // See Migrations folder for migration history

    // Encapsulates all Db operations
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // Configuration for Entity Framework Core
        // Constructor - Pass options back to base class - DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }

        // Creates Categories table in DB - EFC
        public DbSet<Category> Categories { get; set; }

        // Creates Products table in DB
        public DbSet<Product> Products { get; set; }

        // Extending Aspnetusers table in DB
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // Creates ShoppingCarts table in DB
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<OrderHeader> OrderHeaders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Seeds tables - (Setting initial data)
        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            base.OnModelCreating(modelBuilder);

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Sports", DisplayOrder = 1 },
                new Category { CategoryId = 2, Name = "Comedy", DisplayOrder = 2 },
                new Category { CategoryId=3, Name="SciFi", DisplayOrder=3 }
                );

            //Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Title = "Fortune of Time",
                    Author = "Billy Spark",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "SWD9999001",
                    Price = 90,
                    CategoryId = 3,
                    ImageUrl= "\\images\\product\\1133dc89-46e8-47fa-a464-074af0eb26f1.jpg",
                },
                new Product
                {
                    ProductId = 2,
                    Title = "Dark Skies",
                    Author = "Nancy Hoover",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "CAW777777701",
                    Price = 30,
                    CategoryId = 3,
                    ImageUrl = "\\images\\product\\c8393861-6b03-4c06-8abb-a00534878acd.jpg",
                },
                new Product
                {
                    ProductId = 3,
                    Title = "Vanish in the Sunset",
                    Author = "Julian Button",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "RITO5555501",
                    Price = 50,
                    CategoryId = 3,
                    ImageUrl = "\\images\\product\\ddabaa60-7fa1-4bf3-8b1b-5dcdd7de35d9.jpg",
                },
                new Product
                {
                    ProductId = 4,
                    Title = "Cotton Candy",
                    Author = "Abby Muscles",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "WS3333333301",
                    Price = 65,
                    CategoryId = 3,
                    ImageUrl = "\\images\\product\\acb00493-e093-4e55-98ce-482ca3541da6.jpg",
                },
                new Product
                {
                    ProductId = 5,
                    Title = "Rock in the Ocean",
                    Author = "Ron Parker",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "SOTJ1111111101",
                    Price = 27,
                    CategoryId = 3,
                    ImageUrl = "\\images\\product\\275ac629-a433-4617-ad1a-075737f59ff2.jpg",
                },
                new Product
                {
                    ProductId = 6,
                    Title = "Leaves and Wonders",
                    Author = "Laura Phantom",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "FOT000000001",
                    Price = 23,
                    CategoryId = 3,
                    ImageUrl = "\\images\\product\\e8c3c026-2b6f-4e42-93f4-39bf402b1cbe.jpg",
                }
                );

        }

    }
}
