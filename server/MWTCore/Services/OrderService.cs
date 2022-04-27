﻿using MWTCore.Models.Custom;
using MWTCore.Repository.Interfaces;
using MWTCore.Services.Interfaces;
using MWTDbContext.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MWTCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<int> AddCart(int UserID)
        {
            return await _orderRepository.createCart(UserID);
        }

        public async Task<int> AddToCart(CartItem cartItem)
        {
            return await _orderRepository.AddToCart(cartItem);
        }

        public async Task<CartCheckout> cartCheckout(int CartID)
        {
            return await _orderRepository.cartCheckout(CartID);
        }

        public async Task<CartMaster> GetCart(int UserID)
        {
            return await _orderRepository.RetrieveCart(UserID);
        }

        public async Task<bool> isCartAvailable(int id)
        {
            return await _orderRepository.isCartAvailable(id);
        }

        public async Task<bool> PurchaseSuccess(int CartID)
        {
            return await _orderRepository.PurchaseSuccess(CartID);
        }
    }
}