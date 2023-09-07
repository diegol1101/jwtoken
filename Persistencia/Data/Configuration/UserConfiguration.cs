using Dominio.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistencia.Data.Configuration;

    public class UserConfiguration:IEntityTypeConfiguration<User>
    {
        public void Configure (EntityTypeBuilder<User> builder)
        {
            builder.ToTable("user");

            builder.Property(p=> p.UserName)
            .HasColumnName("Username")
            .HasColumnType("Varchar")
            .HasMaxLength(150)
            .IsRequired();







            builder.Property(p=> p.Email)
            .HasColumnName("Email")
            .HasColumnType("Varchar")
            .HasMaxLength(200)
            .IsRequired();

            builder
            .HasMany(p=> p.Rols)
            .WithMany(r=> r.Users)
            .UsingEntity<UserRol>(

                j => j
                .HasOne(pt=> pt.Rol)
                .WithMany(pt => pt.UserRols)
                .HasForeignKey(ut => ut.RolId),

                j => j
                .HasOne(et => et.Usuario)
                .WithMany(et => et.UserRols)
                .HasForeignKey(el=> el.UsuarioId),

                j =>
                {
                    j.ToTable("userRol");
                    j.HasKey(t=> new { t.UsuarioId, t.RolId});
                }
            );
        }
    }
