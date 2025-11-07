using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalog.Infra.DbContexts
{
    public class CatalogContext : DbContext
    {
        public DbSet<ProductEntity> Products { get; set; }

        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductEntity>().HasData(
                new ProductEntity(Guid.Parse("BE056CB4-FB6F-40A6-81EC-4B896072DEE6"), "Lost Hope", "That feeling of hope you've missed during you long walk until this moment in your life", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,1)),
                new ProductEntity(Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"), "Good Feeling", "That feeling once experienced but you can't find it anywhere at all", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,2)),
                new ProductEntity(Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450"), "Night Sleep", "The first night that you've spent awake without any special reason and you regret it doing so", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,3)),
                new ProductEntity(Guid.Parse("102b566b-ba1f-404c-b2df-e2cde39ade09"), "Smile", "Happiness when you laugh about some silly thing", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,4)),
                new ProductEntity(Guid.Parse("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"), "Baffled moment", "Baffled moments of life when you don't know what to say", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,5)),
                new ProductEntity(Guid.Parse("2aadd2df-7caf-45ab-9355-7f6332985a87"), "Childhood", "A small piece of childhood when there weren't any worried", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,6)),
                new ProductEntity(Guid.Parse("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"), "Unicorn Horn", "Magic artifact that grant many power to its carrier", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,7)),
                new ProductEntity(Guid.Parse("5b1c2b4d-48c7-402a-80c3-cc796ad49c6b"), "Memories", "A good memory that has been lost", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11,8)),
                new ProductEntity(Guid.Parse("d8663e5e-7494-4f81-8739-6e0de1bea7ee"), "Rain Drops", "Rain drops during a sleepy night at a cozy home", "1ton", 1, 1, 1, 1, 1, new DateTime(2025,11,9)),

                new ProductEntity(Guid.Parse("34489196-0766-48BB-9918-F36E6AC81DA8"), "Lost Hope 2", "That feeling of hope you've missed during you long walk until this moment in your life", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 1)),
                new ProductEntity(Guid.Parse("73E1B95E-2754-40DE-947E-C1CB4F1E7F67"), "Good Feeling 2", "That feeling once experienced but you can't find it anywhere at all", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 2)),
                new ProductEntity(Guid.Parse("42ED050D-37EC-4C1F-9BFF-84990AC3ADDA"), "Night Sleep 2", "The first night that you've spent awake without any special reason and you regret it doing so", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 3)),
                new ProductEntity(Guid.Parse("498F0EF6-E542-434B-8277-992F770E9118"), "Smile 2", "Happiness when you laugh about some silly thing", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 4)),
                new ProductEntity(Guid.Parse("164A2E03-BD51-4E0F-9E53-B7C181B00104"), "Baffled moment 2", "Baffled moments of life when you don't know what to say", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 5)),
                new ProductEntity(Guid.Parse("EF9A2698-A31E-416E-AC92-F2CA02B5A9D3"), "Childhood 2", "A small piece of childhood when there weren't any worried", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 6)),
                new ProductEntity(Guid.Parse("E96BCC8A-4696-4CD9-A86C-32C8421146F7"), "Unicorn Horn 2", "Magic artifact that grant many power to its carrier", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 7)),
                new ProductEntity(Guid.Parse("EE215607-F2C7-4E48-8DF6-3442D00BDBC7"), "Memories 2", "A good memory that has been lost", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 8)),
                new ProductEntity(Guid.Parse("C8705360-1FC4-4A1A-8380-77C7A2D18B6E"), "Rain Drops 2", "Rain drops during a sleepy night at a cozy home", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 9)),


                new ProductEntity(Guid.Parse("375F3D27-CAEC-4416-A4E6-6D9E5685815E"), "Lost Hope 3", "That feeling of hope you've missed during you long walk until this moment in your life", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 1)),
                new ProductEntity(Guid.Parse("20C1BFD6-1241-4A33-BEFD-D6392E23F5D6"), "Good Feeling 3", "That feeling once experienced but you can't find it anywhere at all", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 2)),
                new ProductEntity(Guid.Parse("E6AF373B-C907-4C48-968E-3879A234892B"), "Night Sleep 3", "The first night that you've spent awake without any special reason and you regret it doing so", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 3)),
                new ProductEntity(Guid.Parse("7E3F11F6-3493-4799-8D24-FC5C7EA31F6A"), "Smile 3", "Happiness when you laugh about some silly thing", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 4)),
                new ProductEntity(Guid.Parse("DAE0CEDB-E1BD-40EE-A6CD-C32732446114"), "Baffled moment 3", "Baffled moments of life when you don't know what to say", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 5)),
                new ProductEntity(Guid.Parse("94635DC2-0CC8-4AB5-B8C0-DD01AB4D30F4"), "Childhood 3", "A small piece of childhood when there weren't any worried", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 6)),
                new ProductEntity(Guid.Parse("F93923A5-3D34-4DF0-BE32-4237DDA10D41"), "Unicorn Horn 3", "Magic artifact that grant many power to its carrier", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 7)),
                new ProductEntity(Guid.Parse("17F181FA-D2C7-4A1A-9848-163958E8B771"), "Memories 3", "A good memory that has been lost", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 8)),
                new ProductEntity(Guid.Parse("E3B711C8-9E11-4B6D-BDD9-1CF4EB40BADE"), "Rain Drops 3", "Rain drops during a sleepy night at a cozy home", "1ton", 1, 1, 1, 1, 1, new DateTime(2025, 11, 9))


                );

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entityTyoe in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityTyoe.ClrType.GetProperties()
                        .Where(p => p.PropertyType == typeof(DateTimeOffset) 
                        || p.PropertyType == typeof(DateTimeOffset?));

                    foreach (var property in properties)
                    {
                        modelBuilder.Entity(entityTyoe.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
        }
    }
}
