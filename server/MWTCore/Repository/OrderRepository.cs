﻿using Microsoft.EntityFrameworkCore;
using MWTCore.Models.Custom;
using MWTCore.Repository.Interfaces;
using MWTDbContext;
using MWTDbContext.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWTCore.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly StoreAppDbCon _context;
        public OrderRepository(StoreAppDbCon context)
        {
            _context = context;
        }

        public async Task<int> UpdateCart(CartItemModel cartItem)
        {
            var item = await _context.cartItems.FirstOrDefaultAsync(ci => ci.CartID == cartItem.CartID && ci.ProductID == cartItem.ProductID);

            int result = 0;

            if (cartItem.Count == 0 && item != null)
            {
                _context.cartItems.Remove(item);
                result = -1;
            }
            else if (item != null && cartItem.Count > 0)
            {
                item.Count += cartItem.Count;
                result = 1;
            }
            else if (item == null && cartItem.Count > 0)
            {
                _context.cartItems.Add(new CartItem
                {
                    CartID = cartItem.CartID,
                    OfferID = cartItem.OfferID,
                    Count = cartItem.Count,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    ProductID = cartItem.ProductID,
                });
                result = 1;
            }

            if (result != 0)
                await _context.SaveChangesAsync();

            return result;
        }

        public async Task<CartCheckout> cartCheckout(int cartID)
        {
            var checkout = new CartCheckout();
            var ProdList = new List<ProductMaster>();
            var StockList = new List<StockMaster>();
            var ProdCountList = new List<int>();
            var OfferList = new List<OfferMaster>();
            int TotalCost = 0;
            int TotalItems = 0;

            var productsInCart = await _context.cartItems.Where(ci => ci.CartID == cartID).ToListAsync();

            foreach (var cartItem in productsInCart)
            {
                var product = await _context.productMasters.FirstAsync(pm => pm.id == cartItem.ProductID);
                ProdList.Add(product);

                ProdCountList.Add(cartItem.Count);

                TotalItems += cartItem.Count;

                var stock = await _context.stockMasters.FirstAsync(sm => sm.ProductID == cartItem.ProductID);
                StockList.Add(stock);

                var offer = await _context.offerMasters.FirstOrDefaultAsync(om => om.ProductID == cartItem.ProductID);
                if (offer != null)
                {
                    OfferList.Add(offer);
                    TotalCost += ((int)Math.Round((product.ProdPrice - product.ProdPrice * ((float)offer.Offer / 100)) * cartItem.Count));
                }
                else
                {
                    OfferList.Add(new OfferMaster());
                    TotalCost += product.ProdPrice * cartItem.Count;
                }
            }

            checkout.TotalCost = TotalCost;
            checkout.TotalItems = TotalItems;
            checkout.Products = ProdList;
            checkout.StockInfo = StockList;
            checkout.ProductCounts = ProdCountList;
            checkout.Offers = OfferList;

            return checkout;

        }

        public async Task<int> createCart(int UserID)
        {
            var cart = new CartMaster();
            cart.UserID = UserID;
            cart.isPaid = false;
            cart.updatedAt = DateTime.Now;
            cart.createdAt = DateTime.Now;
            cart.CartID = await lastCartNumber() + 1;

            _context.cartMasters.Add(cart);
            return await _context.SaveChangesAsync() == 1 ? cart.id : 0;
        }

        public async Task<bool> isCartAvailable(int id)
        {
            return await _context.cartMasters.AsNoTracking().AnyAsync(cm => cm.UserID == id && cm.isPaid == false);
        }

        public async Task<List<OrderHistory>> OrderHistory(int UserID)
        {

            var ReturnHistory = new List<OrderHistory>();

            var PaidCarts = await _context.cartMasters.AsNoTracking().Where(cm => cm.UserID == UserID && cm.isPaid == true).ToListAsync();

            foreach (var cart in PaidCarts)
            {
                var cartID = cart.CartID;
                var cartItems = await _context.cartItems.AsNoTracking().Where(ci => ci.CartID == cartID).ToListAsync();
                foreach (var item in cartItems)
                {
                    var product = await _context.productMasters.AsNoTracking().FirstAsync(pm => pm.id == item.ProductID);
                    var offer = await _context.offerMasters.AsNoTracking().FirstOrDefaultAsync(om => om.id == item.OfferID && om.ProductID == product.id);

                    var History = new OrderHistory();
                    History.Count = item.Count;
                    History.cartID = cartID;
                    History.ProductID = item.ProductID;
                    if (offer == null)
                    {
                        History.Cost = product.ProdPrice;
                    }
                    else
                    {
                        History.Cost = ((int)Math.Round(product.ProdPrice - product.ProdPrice * ((float)offer.Offer / 100)));
                    }
                    History.OrderID = cart.OrderID;
                    History.OrderedOn = cart.updatedAt;

                    ReturnHistory.Add(History);
                }
            }

            return ReturnHistory;
        }

        public async Task<int> PurchaseSuccess(int cartID)
        {
            var checkout = cartCheckout(cartID).Result;

            if (checkout.TotalItems > 0)
            {
                var cartRecord = await _context.cartMasters.FirstAsync(cm => cm.CartID == cartID);

                cartRecord.OrderID = await lastOrderNumber() + 1;
                cartRecord.isPaid = true;
                cartRecord.updatedAt = DateTime.Now;

                foreach (var product in checkout.Products)
                {
                    var stock = _context.stockMasters.Where(sm => sm.ProductID == product.id).FirstOrDefault();
                    if (stock.Stock > 0)
                    {
                        stock.Stock -= checkout.ProductCounts[checkout.Products.FindIndex(fi => fi.id == product.id)];
                    }
                    else
                    {

                        return -1;
                    }
                }

                var updates = await _context.SaveChangesAsync();

                return updates == 1 + checkout.Products.Count ? cartRecord.OrderID : 0;
            }

            else
            {
                return -2;
            }


        }

        public async Task<CartMaster> RetrieveCart(int UserID)
        {
            return await _context.cartMasters.AsNoTracking().Where(cm => cm.UserID == UserID).OrderByDescending(or => or.CartID).FirstOrDefaultAsync();
        }

        private async Task<int> lastCartNumber()
        {
            var number = await _context.cartMasters.AsNoTracking().OrderByDescending(cm => cm.CartID).FirstOrDefaultAsync();

            return number == null ? 0 : number.CartID;

        }
        private async Task<int> lastOrderNumber()
        {
            var number = await _context.cartMasters.AsNoTracking().OrderByDescending(cm => cm.OrderID).FirstOrDefaultAsync();

            return number == null ? 0 : number.OrderID;

        }

        public async Task<object> CompanyHistory(int SellerID)
        {
            return await
                  (from pm in _context.productMasters
                   join ci in _context.cartItems on pm.id equals ci.ProductID
                   join cm in _context.cartMasters on ci.CartID equals cm.id
                   join us in _context.users on cm.UserID equals us.id
                   join ct in _context.categoryMasters on pm.CategoryID equals ct.id
                   join om in _context.offerMasters on ci.OfferID equals om.id into Offers
                   from Offer in Offers.DefaultIfEmpty()
                   where pm.SellerID == SellerID
                   select new
                   {
                       ProductID = pm.id,
                       ProdCompanyName = pm.ProdCompanyName,
                       ProdName = pm.ProdName,
                       ProdDetails = pm.ProdDetails,
                       ProdImage = pm.ProdImage,
                       ProdPrice = pm.ProdPrice,
                       userID = us.id,
                       UserFullname = us.Fullname,
                       CartCount = ci.Count,
                       CategoryName = ct.categoryName,
                       Offer = Offer.Offer
                   })
                   .ToListAsync();

        }
    }
}
