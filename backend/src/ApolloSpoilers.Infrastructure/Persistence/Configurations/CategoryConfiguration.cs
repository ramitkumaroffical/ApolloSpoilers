using ApolloSpoilers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApolloSpoilers.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.Name).HasMaxLength(100).IsRequired();
        b.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        b.HasIndex(c => c.Slug).IsUnique();

        b.HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Name).HasMaxLength(200).IsRequired();
        b.Property(p => p.Slug).HasMaxLength(220).IsRequired();
        b.HasIndex(p => p.Slug).IsUnique();
        b.Property(p => p.Description).HasMaxLength(5000);
        b.Property(p => p.Price).HasColumnType("decimal(18,2)");
        b.Property(p => p.CompareAtPrice).HasColumnType("decimal(18,2)");
        b.Property(p => p.Material).HasMaxLength(100);
        b.Property(p => p.Color).HasMaxLength(60);
        b.Property(p => p.CarBrand).HasMaxLength(80);
        b.Property(p => p.CarModel).HasMaxLength(80);

        b.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.Inventory)
            .WithOne(i => i.Product)
            .HasForeignKey<Inventory>(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.HasKey(i => i.Id);
        b.Property(i => i.ImageUrl).HasMaxLength(500).IsRequired();
        b.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> b)
    {
        b.HasKey(i => i.Id);
        b.HasIndex(i => i.ProductId).IsUnique();
    }
}

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> b)
    {
        b.HasKey(c => c.Id);
        b.HasIndex(c => c.UserId).IsUnique();
        b.HasOne(c => c.User).WithOne().HasForeignKey<Cart>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> b)
    {
        b.HasKey(i => i.Id);
        b.HasOne(i => i.Cart).WithMany(c => c.Items).HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> b)
    {
        b.HasKey(w => w.Id);
        b.HasIndex(w => w.UserId).IsUnique();
        b.HasOne(w => w.User).WithOne().HasForeignKey<Wishlist>(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> b)
    {
        b.HasKey(i => i.Id);
        b.HasOne(i => i.Wishlist).WithMany(w => w.Items).HasForeignKey(i => i.WishlistId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.HasKey(o => o.Id);
        b.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        b.HasIndex(o => o.OrderNumber).IsUnique();
        b.Property(o => o.Subtotal).HasColumnType("decimal(18,2)");
        b.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        b.HasIndex(o => o.UserId);
        b.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.HasKey(i => i.Id);
        b.Property(i => i.ProductName).HasMaxLength(200);
        b.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        b.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.HasKey(r => r.Id);
        b.Property(r => r.Rating).IsRequired();
        b.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
        b.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> b)
    {
        b.HasKey(s => s.Id);
        b.HasIndex(s => s.UserId);
        b.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> b)
    {
        b.HasKey(m => m.Id);
        b.HasOne(m => m.Session).WithMany(s => s.Messages).HasForeignKey(m => m.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiKnowledgeChunkConfiguration : IEntityTypeConfiguration<AiKnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<AiKnowledgeChunk> b)
    {
        b.HasKey(c => c.Id);
        b.HasIndex(c => c.QdrantPointId).IsUnique();
        b.HasIndex(c => c.ProductId);
    }
}
