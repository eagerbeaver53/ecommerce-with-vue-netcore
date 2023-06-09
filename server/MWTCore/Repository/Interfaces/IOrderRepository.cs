﻿using MWTCore.Models.Custom;
using MWTDbContext.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MWTCore.Repository.Interfaces
{
    public interface IOrderRepository
    {
        public Task<bool> isCartAvailable(int id);

        public Task<int> createCart(int UserID);

        public Task<CartMaster> RetrieveCart(int UserID);

        public Task<int> UpdateCart(CartItemModel cartItem);

        public Task<CartCheckout> cartCheckout(int cartID);

        public Task<int> PurchaseSuccess(int cartID);

        public Task<List<OrderHistory>> OrderHistory(int UserID);

        public Task<object> CompanyHistory(int SellerID);
    }
}
