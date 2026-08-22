using Microsoft.EntityFrameworkCore;
using Projeto_Integrador2.Domain;

namespace Projeto_Integrador2.Persistence;

public sealed class ReservationDbContext(DbContextOptions<ReservationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();
    public DbSet<ResourceEntity> Resources => Set<ResourceEntity>();
    public DbSet<RoomResourceEntity> RoomResources => Set<RoomResourceEntity>();
    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();
    public DbSet<ReservationOccurrenceEntity> ReservationOccurrences => Set<ReservationOccurrenceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Role).HasConversion<string>();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<RoomEntity>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(room => room.Id);
            entity.Property(room => room.Capacity).IsRequired();
        });

        modelBuilder.Entity<ResourceEntity>(entity =>
        {
            entity.ToTable("resources");
            entity.HasKey(resource => resource.Id);
        });

        modelBuilder.Entity<RoomResourceEntity>(entity =>
        {
            entity.ToTable("room_resources");
            entity.HasKey(link => new { link.RoomId, link.ResourceId });
            entity.HasOne(link => link.Room).WithMany(room => room.Resources).HasForeignKey(link => link.RoomId);
            entity.HasOne(link => link.Resource).WithMany(resource => resource.Rooms).HasForeignKey(link => link.ResourceId);
        });

        modelBuilder.Entity<ReservationEntity>(entity =>
        {
            entity.ToTable("reservations");
            entity.HasKey(reservation => reservation.Id);
            entity.Property(reservation => reservation.Status).HasConversion<string>();
            entity.HasIndex(reservation => new { reservation.RoomId, reservation.Status });
            entity.HasOne(reservation => reservation.Requester).WithMany(user => user.Reservations).HasForeignKey(reservation => reservation.RequesterId);
            entity.HasOne(reservation => reservation.Room).WithMany(room => room.Reservations).HasForeignKey(reservation => reservation.RoomId);
        });

        modelBuilder.Entity<ReservationOccurrenceEntity>(entity =>
        {
            entity.ToTable("reservation_occurrences");
            entity.HasKey(occurrence => occurrence.Id);
            entity.HasIndex(occurrence => new { occurrence.StartsAt, occurrence.EndsAt });
            entity.HasOne(occurrence => occurrence.Reservation).WithMany(reservation => reservation.Occurrences)
                .HasForeignKey(occurrence => occurrence.ReservationId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}