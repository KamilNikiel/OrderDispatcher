using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderDispatcher.Core.Dtos;
using System.Text.Json;

namespace OrderDispatcher.Core.Database
{
    public class OrderDispatcherSagaStateMap : SagaClassMap<OrderDispatcherSagaState>
    {
        protected override void Configure(EntityTypeBuilder<OrderDispatcherSagaState> entity, ModelBuilder model)
        {
            entity.ToTable("OrderDispatcherSagaStates");

            entity.Property(x => x.CorrelationId).ValueGeneratedNever();
            entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OrderId).HasMaxLength(128);
            entity.Property(x => x.WarehouseDocumentId).HasMaxLength(128);
            entity.Property(x => x.UpdatedAt);

            entity.HasKey(x => x.CorrelationId);
            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.WarehouseDocumentId);
            entity.Property(x => x.OrderData)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<OrderDto>(v, (JsonSerializerOptions?)null)
                )
                .HasColumnType("nvarchar(max)");
        }
    }
}