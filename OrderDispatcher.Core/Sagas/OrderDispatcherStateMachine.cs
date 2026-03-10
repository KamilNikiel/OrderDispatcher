using MassTransit;
using OrderDispatcher.Core.Contracts;
using System;

namespace OrderDispatcher.Core.Sagas
{
    public class OrderDispatcherStateMachine : MassTransitStateMachine<OrderDispatcherSagaState>
    {
        public State WaitingForWarehouseDoc { get; private set; } = null!;
        public State WaitingForOrder { get; private set; } = null!;
        public State WarehouseDocMigrated { get; private set; } = null!;

        public Event<OrderReceivedEvent> OrderReceived { get; private set; } = null!;
        public Event<WarehouseDocumentIssuedEvent> WarehouseDocumentIssued { get; private set; } = null!;

        public OrderDispatcherStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderReceived, x => x
                .CorrelateBy(saga => saga.OrderId, context => context.Message.OrderId)
                .SelectId(context => context.Message.CorrelationId));

            Event(() => WarehouseDocumentIssued, x => x
                .CorrelateBy(saga => saga.OrderId, context => context.Message.OrderId));

            Initially(
                When(OrderReceived)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.OrderData = context.Message.OrderData;
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(WaitingForWarehouseDoc),

                When(WarehouseDocumentIssued)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.WarehouseDocumentId = context.Message.WarehouseDocumentId;
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(WaitingForOrder)
            );

            During(WaitingForWarehouseDoc,
                When(WarehouseDocumentIssued)
                    .Then(context =>
                    {
                        context.Saga.WarehouseDocumentId = context.Message.WarehouseDocumentId;
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .Publish(context => new MigrateToProductionCommand(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Message.WarehouseDocumentId,
                        context.Saga.OrderData!
                    ))
                .Finalize()
            );

            During(WaitingForOrder,
                When(OrderReceived)
                    .Then(context =>
                    {
                        context.Saga.OrderData = context.Message.OrderData;
                        context.Saga.UpdatedAt = DateTime.UtcNow;
                    })
                    .Publish(context => new MigrateToProductionCommand(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Saga.WarehouseDocumentId!,
                        context.Message.OrderData!
                    ))
                .Finalize()
            );
        }
    }
}