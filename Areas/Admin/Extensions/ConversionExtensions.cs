using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memberships.Areas.Admin.Models;
using System.Web;
using Memberships.Entities;
using Memberships.Models;
using System.Data.Entity;
using System.Transactions;

namespace Memberships.Areas.Admin.Extensions
{
    public static class ConversionExtensions
    {
        #region Product
        public static async Task<IEnumerable<ProductModel>> Convert(this IEnumerable<Product> products, ApplicationDbContext db)
        {
            if (products.Count().Equals(0)) return new List<ProductModel>();
                var texts = await db.ProductLinkTexts.ToListAsync();
            var types = await db.ProductTypes.ToListAsync();

            return from p in products
                   select new ProductModel
                   {
                       Id = p.Id,
                       Title = p.Title,
                       Description = p.Description,
                       ImageUrl = p.ImageUrl,
                       ProductLinkTextId = p.ProductLinkTextId,
                       ProductTypeId = p.ProductTypeId,
                       ProductLinkTexts = texts,
                       ProductTypes = types
                   };
        }

        #endregion

        #region Product Item
        public static async Task<IEnumerable<ProductItemModel>> Convert(
            this IQueryable<ProductItem> productItems, ApplicationDbContext db)
        {
            if (productItems.Count().Equals(0))
                return new List<ProductItemModel>();

            return await (from pi in productItems
                          select new ProductItemModel
                          {
                              ItemId = pi.ItemId,
                              ProductId = pi.ProductId,
                              ItemTitle = db.Items.FirstOrDefault(
                                  i=>i.Id.Equals(pi.ItemId)).Title,
                              ProductTitle = db.Products.FirstOrDefault(
                                  p => p.Id.Equals(pi.ProductId)).Title
                          }).ToListAsync();

        }
     

     
        public static async Task<ProductItemModel> Convert(
            this ProductItem productItem, ApplicationDbContext db,
            bool addListData = true)
        {
            var model = new ProductItemModel
            {
                ItemId = productItem.ItemId,
                ProductId = productItem.ProductId,
                Items = addListData ? await db.Items.ToListAsync() : null,
                Products = addListData ? await db.Products.ToListAsync() : null,
                ItemTitle = (await db.Items.FirstOrDefaultAsync( i =>
                    i.Id.Equals(productItem.ItemId))).Title,
                ProductTitle = (await db.Products.FirstOrDefaultAsync( p =>
                    p.Id.Equals(productItem.ProductId))).Title
            };

            return model;
        }

        public static async Task<bool> Canchange(this ProductItem productItem, ApplicationDbContext db)
        {
            var oldPI = await db.ProductItems.CountAsync(pi =>
            pi.ProductId.Equals(productItem.OldProductId) &&
            pi.ItemId.Equals(productItem.OldItemId));

            var newPI = await db.ProductItems.CountAsync(pi =>
            pi.ProductId.Equals(productItem.ProductId) &&
            pi.ItemId.Equals(productItem.ItemId));

            return oldPI.Equals(1) && newPI.Equals(0);
        }

        public static async Task Change(this ProductItem productItem, ApplicationDbContext db)
        {
            var oldProductItem = await db.ProductItems.FirstOrDefaultAsync(
                pi => pi.ProductId.Equals(productItem.OldProductId) &&
                pi.ItemId.Equals(productItem.OldItemId));

            var newProductItem = await db.ProductItems.FirstOrDefaultAsync(
                pi => pi.ProductId.Equals(productItem.OldProductId) &&
                pi.ItemId.Equals(productItem.ItemId));

            if (oldProductItem != null && newProductItem == null)
            {
                newProductItem = new ProductItem
                {
                    ItemId = productItem.ItemId,
                    ProductId = productItem.ProductId
                };

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        db.ProductItems.Remove(oldProductItem);
                        db.ProductItems.Add(newProductItem);

                        await db.SaveChangesAsync();
                        transaction.Complete();
                    }

                    catch
                    {
                        transaction.Dispose();
                    }

                }
            }
        }

        #endregion

        #region Subscription Product

        public static async Task<IEnumerable<SubscriptionProductModel>> Convert(
            this IQueryable<SubscriptionProduct> SubscriptionProduct, ApplicationDbContext db)
        {
            if (SubscriptionProduct.Count().Equals(0))
                return new List<SubscriptionProductModel>();

            return await (from pi in SubscriptionProduct
                          select new SubscriptionProductModel
                          {
                              SubscriptionId = pi.SubscriptionId,
                              ProductId = pi.ProductId,
                              SubscriptionTitle = db.Subscriptions.FirstOrDefault(
                                  i => i.Id.Equals(pi.SubscriptionId)).Title,
                              ProductTitle = db.Products.FirstOrDefault(
                                  p => p.Id.Equals(pi.ProductId)).Title
                          }).ToListAsync();

        }



        public static async Task<SubscriptionProductModel> Convert(
            this SubscriptionProduct SubscriptionProduct, ApplicationDbContext db,
            bool addListData = true)
        {
            var model = new SubscriptionProductModel
            {
                SubscriptionId = SubscriptionProduct.SubscriptionId,
                ProductId = SubscriptionProduct.ProductId,
                Subscriptions = addListData ? await db.Subscriptions.ToListAsync() : null,
                Products = addListData ? await db.Products.ToListAsync() : null,
                SubscriptionTitle = (await db.Items.FirstOrDefaultAsync(s =>
                   s.Id.Equals(SubscriptionProduct.SubscriptionId))).Title,
                ProductTitle = (await db.Products.FirstOrDefaultAsync(p =>
                   p.Id.Equals(SubscriptionProduct.ProductId))).Title
            };

            return model;
        }

        public static async Task<bool> Canchange(
            this SubscriptionProduct SubscriptionProduct,
            ApplicationDbContext db)
        {
            var oldSP = await db.SubscriptionProducts.CountAsync(sp =>
            sp.ProductId.Equals(SubscriptionProduct.OldProductId) &&
            sp.SubscriptionId.Equals(SubscriptionProduct.OldSubscriptionId));

            var newSP = await db.ProductItems.CountAsync(sp =>
            sp.ProductId.Equals(SubscriptionProduct.ProductId) &&
            sp.ItemId.Equals(SubscriptionProduct.SubscriptionId));

            return oldSP.Equals(1) && newSP.Equals(0);
        }

        public static async Task Change(
            this SubscriptionProduct SubscriptionProduct,
            ApplicationDbContext db)
        {
            var oldSubscriptionProduct = await db.SubscriptionProducts.FirstOrDefaultAsync(
                sp => sp.ProductId.Equals(SubscriptionProduct.OldProductId) &&
                sp.SubscriptionId.Equals(SubscriptionProduct.OldSubscriptionId));

            var newSubscriptionProduct = await db.SubscriptionProducts.FirstOrDefaultAsync(
                sp => sp.ProductId.Equals(SubscriptionProduct.OldProductId) &&
                sp.SubscriptionId.Equals(SubscriptionProduct.SubscriptionId));

            if (oldSubscriptionProduct != null && newSubscriptionProduct == null)
            {
                newSubscriptionProduct = new SubscriptionProduct
                {
                    SubscriptionId = SubscriptionProduct.SubscriptionId,
                    ProductId = SubscriptionProduct.ProductId
                };

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        db.SubscriptionProducts.Remove(oldSubscriptionProduct);
                        db.SubscriptionProducts.Add(newSubscriptionProduct);

                        await db.SaveChangesAsync();
                        transaction.Complete();
                    }

                    catch
                    {
                        transaction.Dispose();
                    }

                }
            }
        }

        #endregion

        
    }
}