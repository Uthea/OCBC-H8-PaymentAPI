using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Controllers;
using PaymentAPI.Data;
using PaymentAPI.Models;
using PaymentAPI.Services;
using Xunit;

namespace PaymentAPI.Tests
{
    public class PaymentDetailControllerTests
    {
        private string dbName = Guid.NewGuid().ToString();

        [Fact]
        public async void Should_ReturnTwo_WhenGetAllPaymentDetails()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("dbName").Options;
            var appDbContext = new AppDbContext(options);
            var paymentDetailService = new PaymentDetailService(appDbContext);
            var paymentDetailController = new PaymentDetailController(paymentDetailService);

            Seed(appDbContext);

            var result = await paymentDetailController.GetAllPaymentDetails();
            var okResponse = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<PaymentDetail>>(okResponse.Value);

            Assert.Equal(2, value.Count);

            Drop(appDbContext);
        }

        [Fact]
        public async void Should_ReturnCorrectPaymentDetail_WhenGetPaymentDetailById()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
            var appDbContext = new AppDbContext(options);
            var paymentDetailService = new PaymentDetailService(appDbContext);
            var paymentDetailController = new PaymentDetailController(paymentDetailService);

            Seed(appDbContext);

            var result = await paymentDetailController.GetPaymentDetail(1);
            var okResponse = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaymentDetail>(okResponse.Value);
            Assert.Equal(1, value.paymentDetailId);

            Drop(appDbContext);
        }

        [Fact]
        public async void Should_ReturnNotFound_WhenGetPaymentDetailById()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
            var appDbContext = new AppDbContext(options);
            var paymentDetailService = new PaymentDetailService(appDbContext);

            var paymentDetailController = new PaymentDetailController(paymentDetailService);

            Seed(appDbContext);

            var result = await paymentDetailController.GetPaymentDetail(10);
            var json = Assert.IsType<JsonResult>(result);
            
            Assert.Equal(404, json.StatusCode);

            Drop(appDbContext);
        }

        [Fact]
        public async void Should_ReturnInsertionSuccess_WhenCreatePaymentDetail()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
            var appDbContext = new AppDbContext(options);
            var paymentDetailService = new PaymentDetailService(appDbContext);
            var paymentDetailController = new PaymentDetailController(paymentDetailService);

            Seed(appDbContext);

            var newPaymentDetail = new PaymentDetail
            {
                cardOwnerName = "test123",
                expirationDate = "06/11/2020",
                securityCode = "455"
            };

            var result = await paymentDetailController.CreatePaymentDetail(newPaymentDetail);
            var json = Assert.IsType<JsonResult>(result);

            Assert.Equal("Successfully inserted new data", json.Value);

            Drop(appDbContext);
        }

        [Fact]
        public async void Should_ChangePaymentDetailValues_WhenUpdatePaymentDetail()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
            var appDbContext = new AppDbContext(options);
            var paymentDetailService = new PaymentDetailService(appDbContext);
            var paymentDetailController = new PaymentDetailController(paymentDetailService);

            Seed(appDbContext);

            var newPaymentDetail = new PaymentDetail
            {
                cardOwnerName = "tesla",
                expirationDate = "06/11/2021",
                securityCode = "476"
            };

            await paymentDetailController.UpdatePaymentDetail(2, newPaymentDetail);
            var result = await paymentDetailController.GetPaymentDetail(2);
            var okResponse = Assert.IsType<OkObjectResult>(result);
            var item = Assert.IsType<PaymentDetail>(okResponse.Value);

            var pairs = new List<Tuple<object, object>>()
            {
                new(item.cardOwnerName, newPaymentDetail.cardOwnerName),
                new(item.expirationDate, newPaymentDetail.expirationDate),
                new(item.securityCode, item.securityCode)
            };

            Assert.All(pairs, p => Assert.Equal(p.Item1, p.Item2));

            Drop(appDbContext);
        }

        [Fact]
        public async void Should_RemovePaymentDetail_WhenDeletePaymentDetail()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
            var appDbContext = new AppDbContext(options);
            var paymentDetailService = new PaymentDetailService(appDbContext);
            var paymentDetailController = new PaymentDetailController(paymentDetailService);

            Seed(appDbContext);

            await paymentDetailController.DeletePaymentDetail(1);
            var result = await paymentDetailController.GetPaymentDetail(1);
            var json = Assert.IsType<JsonResult>(result);
            
            Assert.Equal(404, json.StatusCode);

            Drop(appDbContext);
        }

        private void Seed(AppDbContext context)
        {
            var paymentDetails = new[]
            {
                new PaymentDetail
                {
                    cardOwnerName = "sebastian", expirationDate = "05/05/2021", securityCode = "562",
                    paymentDetailId = 1
                },
                new PaymentDetail
                {
                    cardOwnerName = "lasagna", expirationDate = "06/05/2022", securityCode = "532", paymentDetailId = 2
                }
            };

            context.PaymentDetails.AddRange(paymentDetails);
            context.SaveChanges();
        }

        private void Drop(AppDbContext context)
        {
            context.PaymentDetails.RemoveRange(context.PaymentDetails);
            context.SaveChanges();
        }
    }
}