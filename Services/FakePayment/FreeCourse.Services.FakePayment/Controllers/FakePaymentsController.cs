using FreeCourse.Services.FakePayment.Models;
using FreeCourse.Shared.ControllerBases;
using FreeCourse.Shared.Dtos;
using FreeCourse.Shared.Messages;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FreeCourse.Services.FakePayment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FakePaymentsController : CustomBaseController
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public FakePaymentsController(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> ReceivePayment(PaymentDto paymentDto)
        {
            //paymentDto ile ödeme işlemi gerçekleştir.
            var sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:create-order-service"));
            var createOrderMessageCommand = new CreateOrderMessageCommand()
            {
                BuyerId = paymentDto.Order.BuyerId,
                Province = paymentDto.Order.Address.Province,
                District = paymentDto.Order.Address?.District,
                Street = paymentDto.Order.Address.Street,
                Line = paymentDto.Order.Address.Line,
                ZipCode= paymentDto.Order.Address.ZipCode,
            };

            paymentDto.Order.OrderItems.ForEach(x =>
            {
                createOrderMessageCommand.OrderItems.Add(new OrderItem
                {
                    PictureUrl = x.PictureUrl,
                    Price = x.Price,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                });
            });

            await sendEndPoint.Send<CreateOrderMessageCommand>(createOrderMessageCommand);
            return CreateActionResultInstance(Shared.Dtos.Response<NoContent>.Success(200));
        }
    }
}