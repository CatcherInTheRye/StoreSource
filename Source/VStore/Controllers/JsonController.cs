using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreLib.Modules.Application;
using StoreLib.Modules.Autorization;
using StoreLib.Modules.Performance;
using StoreLib.Services.Interface;

namespace VStore.Controllers
{
    public class JsonController : BaseController
    {
        #region init

        private IUserService userService;

        public JsonController(IUserService userService)
        {
            this.userService = userService;
        }

        #endregion init

        //AddCommentVote
        [HttpPost, Compress]
        public JsonResult AddCommentVote(long comment_id, bool help)
        {
            try
            {
                Comment comment = generalServiceOld.GetComments().SingleOrDefault(c => c.ID == comment_id);
                if (comment == null) throw new Exception("Comment does not exists.");
                comment.RateComment++;
                if (help) comment.RateHelpComment++;
                generalServiceOld.UpdateComment(comment);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex.Message);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR));
            }
        }

        //ReviewAdd
        [HttpPost, Compress, VauctionAuthorize]
        public JsonResult ReviewAdd(string title, int rateItem, string nickname, string comments, bool recommend, long type_ID, int? iCustom1, long? biCustom1)
        {
            try
            {
                SessionUser user = AppSession.CurrentUser;
                DateTime dateTimeNow = ApplicationHelper.DateTimeNow;
                Comment comment = new Comment { Status_ID = (long)CommentStatuses.Pending, DateIn = dateTimeNow, Title = Server.HtmlEncode(title), Type_ID = type_ID, iCustom1 = iCustom1, biCustom1 = biCustom1, RateItem = rateItem, Comments = Server.HtmlEncode(comments), Recommend = recommend, RateComment = 0, RateHelpComment = 0, User_ID = user.ID, Nickname = Server.HtmlEncode(nickname), Email = user.Email };
                Promotion promotion = invoiceRepository.PromotionsGet().FirstOrDefault(p => p.IsActive && p.StartDate.GetValueOrDefault(dateTimeNow).Date <= dateTimeNow.Date && p.EndDate.GetValueOrDefault(dateTimeNow).Date >= dateTimeNow.Date);
                Discount discount = invoiceService.GeneratePromotionDiscount(user.ID, new List<long> { comment.biCustom1.GetValueOrDefault(-1) }, promotion);
                generalServiceOld.UpdateComment(comment);
                if (comment.iCustom1 == (int)ObjectTypes.Product)
                {
                    if (promotion != null)
                    {
                        Auction product = auctionRepository.GetAuctionsList().SingleOrDefault(a => a.ID == comment.biCustom1);
                        if (discount != null && product != null)
                        {
                            string dscType;
                            switch ((DiscountType)discount.Type_ID)
                            {
                                case DiscountType.AssignedToShipping:
                                    dscType = "Shipping";
                                    break;
                                case DiscountType.AssignedToSKU:
                                    List<string> products = invoiceRepository.GetDiscountProducts(promotion.Discount.ID, null).Select(p => p.ProductTitle).Distinct().ToList();
                                    dscType = string.Join(", ", products.ToArray());
                                    break;
                                default:
                                    dscType = "Order subtotal";
                                    break;
                            }
                            Mail.SendWriteReviewGetDiscount(user.Email, discount.CouponCode, dscType, discount.DiscountValueText, discount.UnlimitedTime ? string.Empty : string.Format("from {0} to {1}", discount.StartDate.GetValueOrDefault(dateTimeNow).ToShortDateString(), discount.EndDate.GetValueOrDefault(dateTimeNow).ToShortDateString()));
                            PromotionUserLimitation pul = new PromotionUserLimitation { User_ID = user.ID, Promotion_ID = promotion.ID };
                            invoiceRepository.PromotionUserLimitationUpdate(pul);
                        }
                    }
                }
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex.Message);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR));
            }
        }

        //ProductListGet
        [HttpPost, Compress]
        public JsonResult ProductListGet(long categorymap_id, long maincategory_id, long brand_id, int pagesize, int page, string mf, string f, string sortby, string search, decimal minprice, decimal maxprice, string classname)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int imageSize = 0;
            int mod = 1;
            int totalPages = 1;
            List<ProductShortRange> products = new List<ProductShortRange>();
            List<FilterCount> openFilter = new List<FilterCount>();
            try
            {
                SessionUser currentUser = AppHelper.CurrentUser;
                List<ProductShortRange> allproducts = string.IsNullOrEmpty(search) ? productService.ProductShortGetByCategoryMapBrand(categorymap_id, maincategory_id, currentUser != null ? currentUser.Group_ID : 1, brand_id == -1 ? (long?)null : brand_id, false, true) : productService.ProductShortGetBySimpleCriterias(search, currentUser != null ? currentUser.Group_ID : 1);
                products = new List<ProductShortRange>(allproducts.Where(t => (t.MinPrice >= minprice && t.MinPrice <= maxprice) || (t.MaxPrice >= minprice && t.MaxPrice <= maxprice) || (t.MinPrice <= minprice && t.MaxPrice >= maxprice)));
                List<long> filterDO = string.IsNullOrEmpty(mf) ? new List<long>() : mf.Split(',').Where(t => !string.IsNullOrEmpty(t)).ToList().ConvertAll(Convert.ToInt64);
                List<string> filtersList = string.IsNullOrEmpty(f) ? new List<string>() : f.Split(',').ToList();
                Dictionary<long, List<long>> applyFilters = new Dictionary<long, List<long>>();
                foreach (string filter in filtersList)
                {
                    List<string> currentfilter = filter.Split('_').ToList();
                    if (currentfilter.Count < 2) continue;
                    long attributetype_id = long.Parse(currentfilter[0]);
                    long attributevalue_id = long.Parse(currentfilter[1]);
                    if (!applyFilters.ContainsKey(attributetype_id))
                    {
                        applyFilters.Add(attributetype_id, new List<long> { attributevalue_id });
                        continue;
                    }
                    List<long> t = applyFilters[attributetype_id];
                    t.Add(attributevalue_id);
                    applyFilters[attributetype_id] = t;
                }
                openFilter = productService.ListFiltersGet(categorymap_id, maincategory_id, brand_id == -1 ? (long?)null : brand_id, true);
                foreach (long applyType_id in filterDO)
                {
                    List<ProductShortRange> tmp = new List<ProductShortRange>(products);
                    products = tmp.Where(t => t.FiltersList.ContainsKey(applyType_id) && applyFilters[applyType_id].Intersect(t.FiltersList[applyType_id]).Any()).ToList();
                    foreach (FilterCount filterCount in openFilter.Where(t => t.AttributeType_ID == applyType_id))
                    {
                        filterCount.Count = tmp.Except(products).Count(t => t.FiltersList.ContainsKey(applyType_id) && t.FiltersList[applyType_id].Contains(filterCount.AttributeValue_ID));
                    }
                }
                foreach (FilterCount filterCount in openFilter.Where(t => !filterDO.Contains(t.AttributeType_ID)))
                {
                    filterCount.Count = products.Count(t => t.FiltersList.ContainsKey(filterCount.AttributeType_ID) && t.FiltersList[filterCount.AttributeType_ID].Contains(filterCount.AttributeValue_ID));
                }
                totalPages = (int)Math.Ceiling((float)products.Count / (float)pagesize);
                imageSize = allproducts.Count() > ApplicationHelper.PageSize ? AppHelper.ThumbnailMediumSize : ApplicationHelper.ImageSmallSize;
                mod = allproducts.Count() > ApplicationHelper.PageSize ? 4 : 3;
                long sort;
                long.TryParse(sortby, out sort);
                switch (sort)
                {
                    case (long)CategorySortFields.Newest:
                        products = products.OrderByDescending(t => t.DateIn).ThenBy(t => t.Product.Title).ToList();
                        break;
                    case (long)CategorySortFields.Oldest:
                        products = products.OrderBy(t => t.DateIn).ThenBy(t => t.Product.Title).ToList();
                        break;
                    case (long)CategorySortFields.PriceLowToHigh:
                        products = products.OrderBy(t => t.MinPrice).ThenBy(t => t.Product.Title).ToList();
                        break;
                    case (long)CategorySortFields.PriceHighToLow:
                        products = products.OrderByDescending(t => t.MaxPrice).ThenBy(t => t.Product.Title).ToList();
                        break;
                    case (long)CategorySortFields.Title:
                        products = products.OrderBy(t => t.Product.Title).ToList();
                        break;
                    default:
                        products = products.OrderBy(t => t.Priority).ThenBy(t => t.Product.Title).ToList();
                        break;
                }
                products = products.Skip((page - 1) * pagesize).Take(pagesize).ToList();
            }
            catch (Exception exc)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, exc.Message, "PRODUCT LIST");
            }
            return JSON(new JsonTemplateResult<ProductView, FilterCount>
            {
                CurrentPage = page,
                TotalPage = totalPages,
                Templates = products.Select((t, i) => new ProductView { DivClassName = string.Format(classname), ProductTitle = t.Product.Title, ProductPrice = string.Format("{0}{1}", t.MinPrice.GetCurrency(false), t.MinPrice != t.MaxPrice ? " - " + t.MaxPrice.GetCurrency(false) : string.Empty), ProductShortDesc = t.ShortDescription, ProductImageSize = imageSize.ToString(), ProductImagePath = !string.IsNullOrEmpty(t.ThumbnailImage) ? ApplicationHelper.Urls.ProductImage(t.Product.ID, t.ThumbnailImage, imageSize, imageSize) : ApplicationHelper.Urls.CompressImageFrontEnd("image_coming_soon.jpg"), ProductLink = Url.Action("ProductDetail", new { controller = "Catalog", id = t.Product.ID, param1 = UrlParser.TitleToUrl(t.FullCategoryTitle), param2 = UrlParser.TitleToUrl(t.Product.Title), s = t.Separator ? (long?)t.SKU.ID : null }) }).ToArray(),
                Objects = openFilter,
                Result = result
            });
        }

        #region cart
        //CartsGetTotal
        private static void CartsGetTotal(out decimal total, out int count)
        {
            SessionShoppingCart cart = AppHelper.Cart ?? new SessionShoppingCart();
            SessionPackageCart cartPackage = AppHelper.CartPackage ?? new SessionPackageCart();
            SessionGiftCart giftCart = AppHelper.GiftCart ?? new SessionGiftCart();
            total = cart.TotalPrice + cartPackage.TotalPrice + giftCart.TotalPrice;
            count = cart.Lines.Count + cartPackage.Lines.Count + giftCart.Lines.Count;
        }

        //AddItem
        private void AddItem(long product_id, long sku_id, int qty, int? backQty, long? user_id, long usergroup_id)
        {
            ProductInformation product = productService.ProductInformationGet(product_id, usergroup_id, true);
            if (product == null) throw new Exception("You can't add this item to cart, because it doesn't exist in the system.");
            SKUDetail sku = product.SKUs.FirstOrDefault(s => s.IdTitle.ID == sku_id);
            if (sku == null) throw new Exception("You can't add this item to cart, because it doesn't exist in the system.");
            if (sku.MinOrderQty > (qty + backQty.GetValueOrDefault(0))) throw new Exception("Min quantity for order: " + sku.MinOrderQty);
            if ((sku.Quantity == 0 && !product.IsBackorder) || (sku.MinOrderQty > sku.Quantity && !product.IsBackorder)) throw new Exception("You can't add this item to cart, because it is out of stock.");
            ProductShortRange pshort = productService.ProductShortRangeGet(sku_id, usergroup_id, true);
            if (pshort == null) throw new Exception("You can't add this item to cart, because it doesn't exist in the system.");
            AppHelper.Cart.AddItem(pshort, sku, product.SKUAttr.Where(s => s.SKU_ID == sku_id).ToList(), qty, product.IsBackorder ? backQty.HasValue ? backQty.Value : sku.Quantity < qty && product.IsBackorder ? qty - sku.Quantity : 0 : 0);
            if (user_id.HasValue) invoiceRepository11.ShoppingCartAddItem(new ShoppingCart { User_ID = user_id.Value, ObjectType_ID = (long)ObjectTypes.Product, Object_ID = sku_id, Quantity = AppHelper.Cart.Lines[sku_id].TotalQty });
        }

        //AddItemForSync
        private void AddItemForSync(long usergroup_id, ShoppingCart shoppingCart)
        {
            switch ((ObjectTypes)shoppingCart.ObjectType_ID)
            {
                case ObjectTypes.Product:
                    SKUDetail sku = auctionRepository.GetSKUDetail(shoppingCart.Object_ID, usergroup_id);
                    if (sku == null || !sku.IsActive)
                    {
                        invoiceRepository11.ShoppingCartRemoveItem(shoppingCart);
                        return;
                    }
                    ProductInformation product = productService.ProductInformationGet(sku.Product.Auction_ID, usergroup_id, true);
                    if (product == null)
                    {
                        invoiceRepository11.ShoppingCartRemoveItem(shoppingCart);
                        return;
                    }
                    if (sku.Quantity == 0 && !product.IsBackorder)
                    {
                        invoiceRepository11.ShoppingCartRemoveItem(shoppingCart);
                        return;
                    }
                    ProductShortRange pshort = productService.ProductShortRangeGet(shoppingCart.Object_ID, usergroup_id, true);
                    if (pshort == null)
                    {
                        invoiceRepository11.ShoppingCartRemoveItem(shoppingCart);
                        return;
                    }
                    AppHelper.Cart.AddItem(pshort, sku, product.SKUAttr.Where(s => s.SKU_ID == shoppingCart.Object_ID).ToList(), shoppingCart.Quantity, sku.Quantity < shoppingCart.Quantity && product.IsBackorder ? shoppingCart.Quantity - sku.Quantity : 0);
                    break;
                case ObjectTypes.GiftCard:
                    GiftCardDetail details = productService.GiftCardsDetailGet(true);
                    if (details == null)
                    {
                        invoiceRepository11.ShoppingCartRemoveItem(shoppingCart);
                        return;
                    }
                    IdTitleValue giftsku = details.GiftSKUs.FirstOrDefault(t => t.ID == shoppingCart.Object_ID);
                    if (giftsku == null)
                    {
                        giftsku = details.GiftSKUs.FirstOrDefault(t => t.Value == 0);
                        if (giftsku == null)
                        {
                            invoiceRepository11.ShoppingCartRemoveItem(shoppingCart);
                            return;
                        }
                    }
                    PasswordGenerator generator = new PasswordGenerator(9, 10);
                    AppHelper.GiftCart.AddItem(new GiftCardLine
                    {
                        SKU_ID = giftsku.ID,
                        UID = generator.Generate(),
                        Image = details.GiftImages.FirstOrDefault() ?? new ImageDetail(),
                        ProductTitle = details.GiftCard.Title,
                        SKU = giftsku.Title,
                        Price = shoppingCart.Price.GetValueOrDefault(0),
                        DeliveryDate = shoppingCart.dtCustom1.GetValueOrDefault(ApplicationHelper.DateTimeNow),
                        Quantity = shoppingCart.Quantity,
                        Message = shoppingCart.vCustom7,
                        Product_ID = details.GiftCard.ID,
                        RecipientFirstName = shoppingCart.vCustom1,
                        RecipientLastName = shoppingCart.vCustom2,
                        RecipientEmail = shoppingCart.vCustom3,
                        SenderFirstName = shoppingCart.vCustom4,
                        SenderLastName = shoppingCart.vCustom5,
                        SenderEmail = shoppingCart.vCustom6,
                    });
                    break;
            }
        }

        //AddItemToCart
        [HttpPost, Compress]
        public JsonResult AddItemToCart(long product_id, long sku_id, int qty)
        {
            JsonExecuteResult result;
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                SessionShoppingCart cart = AppHelper.Cart ?? new SessionShoppingCart();
                SessionPackageCart cartPackage = AppHelper.CartPackage ?? new SessionPackageCart();
                SessionGiftCart giftCart = AppHelper.GiftCart ?? new SessionGiftCart();
                bool add = !cart.Lines.ContainsKey(sku_id);
                AddItem(product_id, sku_id, qty, null, currentuser != null ? (long?)currentuser.ID : null, currentuser != null ? currentuser.Group_ID : 1);
                CartItem cline = cart.Lines[sku_id];
                ShoppingCartChange();
                result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { alertMessage = qty > cline.SKU.Quantity ? "Unfortunately, the amount you want to order is not available at the moment. The quantity was changed to the maximum that we can offer." : null, objCount = (cart.Lines.Count + cartPackage.Lines.Count + giftCart.Lines.Count), total = (cart.TotalPrice + giftCart.TotalPrice + cartPackage.TotalPrice).GetCurrency(false), add = Convert.ToByte(add), linetotal = cline.TotalPrice.GetCurrency(false), qty = cline.Quantity, backQty = cline.BackorderQty, skuPrice = cline.SKU.FinalPrice.GetCurrency(false), sku_title = cline.SKU.IdTitle.Title });
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
            return JSON(result);
        }

        //UpdateItemQtyInCart
        [HttpPost, Compress]
        public JsonResult UpdateItemQtyInCart(long product_id, long sku_id, int qty)
        {
            JsonExecuteResult result;
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                AddItem(product_id, sku_id, qty, null, currentuser != null ? (long?)currentuser.ID : null, currentuser != null ? currentuser.Group_ID : 1);
                SessionShoppingCart cart = AppHelper.Cart;
                CartItem cline = cart.Lines[sku_id];
                ShoppingCartChange();
                decimal total;
                int objCount;
                CartsGetTotal(out total, out objCount);
                result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { itemtotald = cline.TotalPrice.GetCurrency(false), itemtotal = cline.TotalPrice, qty = cline.Quantity, iscustom = "", carttotal = total.GetCurrency(false), backQty = cline.BackorderQty, prtotal = cline.TotalPrice.GetCurrency(false), alertMessage = qty > cline.SKU.Quantity ? "Unfortunately, the amount you want to order is not available at the moment. The quantity was changed to the maximum that we can offer." : null });
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
            return JSON(result);
        }

        //UpdateItemBackQtyInCart
        [HttpPost, Compress]
        public JsonResult UpdateItemBackQtyInCart(long product_id, long sku_id, int backQty)
        {
            JsonExecuteResult result;
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                SessionShoppingCart cart = AppHelper.Cart;
                CartItem cline = cart.Lines[sku_id];
                if (cline == null) throw new Exception("Error during operation. Please reload page and try again.");
                AddItem(product_id, sku_id, cline.Quantity, backQty, currentuser != null ? (long?)currentuser.ID : null, currentuser != null ? currentuser.Group_ID : 1);
                cline = cart.Lines[sku_id];
                ShoppingCartChange();
                decimal total;
                int objCount;
                CartsGetTotal(out total, out objCount);
                result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { itemtotald = cline.TotalPrice.GetCurrency(false), itemtotal = cline.TotalPrice, qty = cline.Quantity, carttotal = total.GetCurrency(false), iscustom = "" });
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
            return JSON(result);
        }

        #region gift cards
        //AddGiftToCart
        [HttpPost, Compress]
        public JsonResult AddGiftToCart(GiftCardLine giftCard, long? user_id)
        {
            JsonExecuteResult result;
            try
            {
                giftCard.Validate(ModelState, ApplicationHelper.DateTimeNow);
                if (!ModelState.IsValid)
                {
                    var errors = (from M in ModelState where M.Value != null && M.Value.Errors.FirstOrDefault() != null && !String.IsNullOrEmpty(M.Value.Errors.FirstOrDefault().ErrorMessage) select new { field = M.Key, message = (M.Value != null && M.Value.Errors.FirstOrDefault() != null) ? M.Value.Errors.FirstOrDefault().ErrorMessage : string.Empty }).ToArray(); throw new Exception(String.Join(" | ", errors.Select(q => q.message)));
                }
                GiftCardDetail details = productService.GiftCardsDetailGet(true);
                if (details == null) throw new Exception("You can't add this item to cart, because it doesn't exist in the system.");
                IdTitleValue sku = details.GiftSKUs.FirstOrDefault(t => t.ID == giftCard.SKU_ID);
                if (sku == null) throw new Exception("You can't add this item to cart, because it doesn't exist in the system.");
                if (sku.Value > 0 && sku.Value != giftCard.Price)
                {
                    sku = details.GiftSKUs.FirstOrDefault(t => t.Value == 0);
                    if (sku == null) throw new Exception("You can't add this item to cart, because it doesn't exist in the system.");
                    giftCard.SKU_ID = sku.ID;
                }
                PasswordGenerator generator = new PasswordGenerator(9, 10);
                giftCard.UID = generator.Generate();
                giftCard.Image = details.GiftImages.FirstOrDefault() ?? new ImageDetail();
                giftCard.ProductTitle = details.GiftCard.Title;
                giftCard.SKU = sku.Title;
                AppHelper.GiftCart.AddItem(giftCard);
                if (user_id.HasValue) invoiceRepository11.ShoppingCartAddItem(new ShoppingCart { User_ID = user_id.Value, ObjectType_ID = (long)ObjectTypes.GiftCard, Object_ID = giftCard.SKU_ID, Quantity = giftCard.Quantity, Price = giftCard.Price, dtCustom1 = giftCard.DeliveryDate, vCustom1 = giftCard.RecipientFirstName, vCustom2 = giftCard.RecipientLastName, vCustom3 = giftCard.RecipientEmail, vCustom4 = giftCard.SenderFirstName, vCustom5 = giftCard.SenderLastName, vCustom6 = giftCard.SenderEmail, vCustom7 = giftCard.Message });
                ShoppingCartChange();
                decimal total;
                int objCount;
                CartsGetTotal(out total, out objCount);
                result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { uid = giftCard.UID, sku_title = giftCard.SKU, skuPrice = giftCard.Price.GetCurrency(false), linetotal = giftCard.TotalPrice.GetCurrency(false), total, objCount });
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
            return JSON(result);
        }

        //UpdateGiftQtyInCart
        [HttpPost, Compress]
        public JsonResult UpdateGiftQtyInCart(string id, int qty)
        {
            JsonExecuteResult result;
            try
            {
                SessionGiftCart giftCart = AppHelper.GiftCart;
                GiftCardLine line = giftCart.Lines[id];
                if (line == null) throw new Exception("Error during operation. Please reload page and try again.");
                line.Quantity = qty;
                decimal total;
                int objCount;
                CartsGetTotal(out total, out objCount);
                result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { itemtotald = line.TotalPrice.GetCurrency(false), carttotal = total.GetCurrency(false), iscustom = "" });
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
            ShoppingCartChange();
            return JSON(result);
        }
        #endregion

        //SyncSessionUserCart
        [NonAction]
        public void SyncSessionUserCart(long user_id, long usergroup_id)
        {
            SessionShoppingCart cart = AppHelper.Cart;
            Dictionary<long, CartItem> itemsInCart = cart.Lines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (KeyValuePair<long, CartItem> kvp in itemsInCart) AddItem(kvp.Value.Product.Product.ID, kvp.Key, kvp.Value.Quantity, kvp.Value.BackorderQty, user_id, usergroup_id);

            List<ShoppingCart> items = invoiceRepository11.ShoppingCartGetItems(user_id);
            foreach (ShoppingCart item in items)
            {
                switch ((ObjectTypes)item.ObjectType_ID)
                {
                    case ObjectTypes.Product:
                        if (itemsInCart.ContainsKey(item.Object_ID)) continue;
                        break;
                    case ObjectTypes.GiftCard:
                        break;
                    default: continue;
                }
                AddItemForSync(usergroup_id, item);
            }
            //TODO sync Packages
        }

        //ShoppingCartRemoveObject
        [HttpPost, Compress]
        public JsonResult ShoppingCartRemoveObject(string object_id, long objectType)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                SessionShoppingCart cart = AppHelper.Cart ?? new SessionShoppingCart();
                SessionPackageCart cartPackage = AppHelper.CartPackage ?? new SessionPackageCart();
                SessionGiftCart giftCart = AppHelper.GiftCart ?? new SessionGiftCart();
                switch ((ObjectTypes)objectType)
                {
                    case ObjectTypes.Product:
                        long id;
                        long.TryParse(object_id, out id);
                        cart.RemoveItem(id);
                        if (currentuser != null) invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_ID = currentuser.ID, ObjectType_ID = objectType, Object_ID = id });
                        break;
                    case ObjectTypes.GiftCard:
                        if (currentuser != null && giftCart.Lines.ContainsKey(object_id)) invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_ID = currentuser.ID, ObjectType_ID = objectType, Object_ID = giftCart.Lines[object_id].SKU_ID, Price = giftCart.Lines[object_id].Price });
                        giftCart.RemoveItem(object_id);
                        break;
                }
                ShoppingCartChange();
                int objCount = cart.Lines.Count + giftCart.Lines.Count + cartPackage.Lines.Count;
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { carttotal = (cart.TotalPrice + cartPackage.TotalPrice + giftCart.TotalPrice).GetCurrency(), iscustom = "", reload = objCount == 0, objCount }));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //ShoppingCartChange
        private void ShoppingCartChange()
        {
            Session["CartPayment"] = null;
        }

        //ClearShoppingCart
        [HttpPost, Compress]
        public JsonResult ClearShoppingCart()
        {
            try
            {
                SessionUser currentUser = AppHelper.CurrentUser;
                SessionShoppingCart cart = AppHelper.Cart;
                SessionPackageCart packageCart = AppHelper.CartPackage;
                SessionGiftCart giftCart = AppHelper.GiftCart;
                if (currentUser != null)
                {
                    foreach (long sku_id in cart.Lines.Keys) invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_ID = currentUser.ID, ObjectType_ID = (long)ObjectTypes.Product, Object_ID = sku_id });
                    foreach (GiftCardLine giftCardLine in giftCart.Lines.Values) invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_ID = currentUser.ID, ObjectType_ID = (long)ObjectTypes.GiftCard, Object_ID = giftCardLine.SKU_ID, Price = giftCardLine.Price });
                }
                cart.ClearCart();
                packageCart.ClearCart();
                giftCart.ClearCart();
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }
        #endregion

        #region WishList

        //AddItemToWishList
        [HttpPost, Compress, VauctionAuthorize]
        public JsonResult AddItemToWishList(long objectType_ID, long object_ID, string wls, string title)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                List<WishList> wishLists = productService.WishListsGetByUser(currentuser.ID, true);
                List<WishListItem> wishListItems = productService.WishListItemsGetByUser(currentuser.ID, true);
                List<IdTitleFlag> wl_ids = new JavaScriptSerializer().Deserialize<List<IdTitleFlag>>(wls) ?? new List<IdTitleFlag>();
                if (!string.IsNullOrEmpty(title)) wl_ids.Add(new IdTitleFlag { ID = -1, Title = title, Flag = true });
                long newID = 0;
                List<IdTitleIdDesc> result = new List<IdTitleIdDesc>();
                if (!wl_ids.Any())
                {
                    WishList wishList = wishLists.FirstOrDefault(t => string.IsNullOrEmpty(t.SecureCode)) ?? productService.WishListUpdate(new WishList { User_ID = currentuser.ID, DateIn = ApplicationHelper.DateTimeNow, SecureCode = string.Empty, Title = "Default wish list" });
                    if (wishListItems.FirstOrDefault(t => t.WishList_ID == wishList.ID && t.ItemType_ID == objectType_ID && t.Object_ID == object_ID) == null) productService.WishListItemUpdate(currentuser.ID, new WishListItem { ItemType_ID = objectType_ID, Object_ID = object_ID, WishList_ID = wishList.ID });
                }
                else
                {
                    foreach (IdTitleFlag wl in wl_ids.Where(t => t.Flag))
                    {
                        WishList wishList = wishLists.FirstOrDefault(t => t.ID == wl.ID);
                        if (wishList == null)
                        {
                            wishList = productService.WishListUpdate(new WishList { User_ID = currentuser.ID, DateIn = ApplicationHelper.DateTimeNow, SecureCode = Guid.NewGuid().ToString().Substring(0, 20).Replace("-", ""), Title = Server.HtmlEncode(wl.Title) });
                            newID = wishList.ID;
                        }
                        int i = 0;
                        if (wishListItems.FirstOrDefault(t => t.WishList_ID == wishList.ID && t.ItemType_ID == objectType_ID && t.Object_ID == object_ID) == null)
                        {
                            i++;
                            productService.WishListItemUpdate(currentuser.ID, new WishListItem { ItemType_ID = objectType_ID, Object_ID = object_ID, WishList_ID = wishList.ID });
                        }
                        result.Add(new IdTitleIdDesc { ID = wishList.ID, Title = wishList.Title, Description = new IdTitle { ID = wishListItems.Count(t => t.WishList_ID == wishList.ID) + i, Title = Url.Action("WishlistDetail", "Account", new { id = wishList.ID }) } });
                    }
                }
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { wishlist = result.Select(t => new { wl_id = t.ID, wl_title = t.Title, wl_count = t.Description.ID, wl_link = t.Description.Title }).ToArray(), newID = newID }));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //WishListDelete
        [HttpPost, Compress, VauctionAuthorize]
        public JsonResult WishListDelete(long id)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                List<WishList> wishLists = productService.WishListsGetByUser(currentuser.ID, true);
                if (wishLists.All(t => t.ID != id)) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
                productService.WishListDelete(currentuser.ID, id);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //WishListUpdate
        [HttpPost, Compress, VauctionAuthorize]
        public JsonResult WishListUpdate(long? id, string title)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                WishList wishList = productService.WishListsGetByUser(currentuser.ID, true).FirstOrDefault(t => t.ID == id.GetValueOrDefault(0)) ?? new WishList { DateIn = ApplicationHelper.DateTimeNow, User_ID = currentuser.ID, SecureCode = Guid.NewGuid().ToString().Substring(0, 20).Replace("-", "") };
                wishList.Title = !string.IsNullOrEmpty(title) ? Server.HtmlEncode(title) : "unnamed";
                wishList = productService.WishListUpdate(wishList);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { wishlist = new object[] { new { wl_id = wishList.ID, wl_title = wishList.Title, wl_link = Url.Action("WishlistDetail", "Account", new { id = wishList.ID }), wl_public = string.Format("{0}{1}", ApplicationHelper.SiteUrl, Url.Action("WishlistP", "Account", new { scode = wishList.SecureCode })) } } }));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //WishListItemDelete
        [HttpPost, Compress, VauctionAuthorize]
        public JsonResult WishListItemDelete(long id)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                List<WishListItem> wishListItems = productService.WishListItemsGetByUser(currentuser.ID, true);
                if (wishListItems.All(t => t.ID != id)) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
                productService.WishListItemDelete(currentuser.ID, new List<long> { id });
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //WishListItemAddToCart
        [HttpPost, Compress]
        public JsonResult WishListItemAddToCart(long id, int qty)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                WishListItem wishListItem = productService.WishListItemsGet().FirstOrDefault(t => t.ID == id);
                if (wishListItem == null) throw new Exception("Please reload the page and try again.");
                string alertMessage = null;
                switch ((ObjectTypes)wishListItem.ItemType_ID)
                {
                    case ObjectTypes.Product:
                        AuctionSKU sku = auctionRepository.GetSKUList().FirstOrDefault(t => t.ID == wishListItem.Object_ID);
                        if (sku == null || !sku.IsActive.GetValueOrDefault(true)) throw new Exception("Please reload the page and try again.");
                        AddItem(sku.Auction_ID, sku.ID, qty, null, currentuser != null ? currentuser.ID : (long?)null, currentuser != null ? currentuser.Group_ID : 1);
                        CartItem cline = AppHelper.Cart.Lines[sku.ID];
                        alertMessage = qty > cline.SKU.Quantity ? "Unfortunately, the amount you want to order is not available at the moment. The quantity was changed to the maximum that we can offer." : null;
                        break;
                }
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { alertMessage = alertMessage }));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //WishListItemsAddAllToCart
        [HttpPost, Compress]
        public JsonResult WishListItemsAddAllToCart(long wishlist_id)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                List<WishListItem> wishListItems = productService.WishListItemsGet().Where(t => t.WishList_ID == wishlist_id).ToList();
                foreach (WishListItem item in wishListItems)
                {
                    switch ((ObjectTypes)item.ItemType_ID)
                    {
                        case ObjectTypes.Product:
                            AuctionSKU sku = auctionRepository.GetSKUList().FirstOrDefault(t => t.ID == item.Object_ID);
                            if (sku == null || !sku.IsActive.GetValueOrDefault(true)) throw new Exception("Please reload the page and try again.");
                            AddItem(sku.Auction_ID, sku.ID, 1, null, currentuser != null ? currentuser.ID : (long?)null, currentuser != null ? currentuser.Group_ID : 1);
                            break;
                    }
                }
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        //WishListItemsDeleteAll
        [HttpPost, Compress, VauctionAuthorize]
        public JsonResult WishListItemsDeleteAll(long wishlist_id)
        {
            try
            {
                SessionUser currentuser = AppHelper.CurrentUser;
                List<long> wishListItems = productService.WishListItemsGetByUser(currentuser.ID, true).Where(t => t.WishList_ID == wishlist_id).Select(t => t.ID).ToList();
                if (wishListItems.Any()) productService.WishListItemDelete(currentuser.ID, wishListItems);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }
        #endregion

        #region checkout
        //SaveShipping
        [HttpPost, Compress]
        public JsonResult SaveShipping(long service_id, decimal shippingCost)
        {
            CartPayment cp = Session["CartPayment"] as CartPayment;
            if (cp == null) return JSON(new { });
            cp.DeliveryService_ID = service_id;
            OrderService shipping = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.Shipping);
            if (shipping == null)
            {
                Service service = productService.GetServices(true).First(s => s.ID == (long)StoreServices.Shipping);
                cp.Services.Add(new OrderService { IdTitle = new IdTitle { ID = service.ID, Title = service.Name }, Value = shippingCost, IsAddInInvoice = true, Adding = true });
            }
            else
            {
                shipping.Value = shippingCost;
            }
            List<OrderService> coupons = cp.Services.Where(s => s.IdTitle.ID == (long)StoreServices.ShippingDiscount).ToList();
            foreach (OrderService coupon in coupons)
            {
                cp.Services.Remove(coupon);
            }
            return JSON(new { total = cp.TotalDue.GetCurrency(), shWithHl = shippingCost.GetCurrency(), discard = coupons.Any() });
        }

        //ApplyDiscount
        /// <summary>
        /// Валидация и применение купона при покупке с основного сайта
        /// </summary>
        /// <param name="discount_code"></param>
        /// <returns></returns>
        [HttpPost, Compress]
        public JsonResult ApplyDiscount(string discount_code)
        {
            try
            {
                if (string.IsNullOrEmpty(discount_code) || discount_code.Equals("Promo Code")) return JSON(null);
                CartPayment cp = Session["CartPayment"] as CartPayment;
                if (cp == null || !cp.ProductLines.Any()) return JSON(null);
                Discount discount = invoiceRepository.GetDiscounts().SingleOrDefault(d => d.CouponCode == discount_code);
                string dscType = string.Empty;
                SessionUser cuser = AppHelper.CurrentUser;
                const string errorMsg = "The Promo Code is invalid.";
                if (discount == null || !discount.IsActive || discount.Type_ID == (long)DiscountType.AssignedToPackage || !(from ugd in invoiceRepository.GetUserGroupDiscounts().Where(ud => ud.Discount_ID == discount.ID) select ugd.UserGroup_ID).Contains(cuser != null ? cuser.Group_ID : userService.GetDefaultRegistrationGroup().ID) || (!discount.UnlimitedTime && (ApplicationHelper.DateTimeNow > discount.EndDate || ApplicationHelper.DateTimeNow < discount.StartDate))) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, errorMsg));
                List<long> uu = (from d in invoiceRepository.GetDiscountUserLimitation()
                                 where d.Discount_ID == discount.ID
                                 orderby d.User_ID
                                 select d.User_ID).Distinct().ToList();
                if (!uu.Contains(cuser != null ? cuser.ID : cp.User.User_ID) && discount.UserAmountLimitation > 0 && uu.Count >= discount.UserAmountLimitation)
                {
                    return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, errorMsg));
                }

                switch ((DiscountLimitation)discount.Limitation_ID)
                {
                    case DiscountLimitation.NTimesOnly:
                        if (invoiceRepository.GetDiscountUserLimitation().Count(d => d.Discount_ID == discount.ID) >= discount.LimitationValue) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, "The Promo Code is invalid.<br/>It has already been used."));
                        break;
                    case DiscountLimitation.NTimePerCustomer:
                        if (invoiceRepository.GetDiscountUserLimitation().Count(d => d.Discount_ID == discount.ID) >= discount.LimitationValue) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, "The Promo Code is invalid.<br/>It has already been used."));
                        break;
                }
                decimal taxServiceValue = 0;
                switch ((DiscountType)discount.Type_ID)
                {
                    case DiscountType.AssignedToSKU:
                        {
                            List<long> cpcSKUids = cp.ProductLines.Where(t => t.Package.ID == 0).Select(l => l.SKU.IdTitle.ID).Distinct().ToList();
                            List<long> discountSKUids = invoiceRepository.GetDiscountSKUs(discount.ID).Select(dp => dp.SKU_ID).ToList();
                            if (!discountSKUids.Intersect(cpcSKUids).Any()) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, errorMsg));
                            dscType = "some product(s) price";
                            List<CartItem> lines = new List<CartItem>(cp.ProductLines);
                            StoreForm defaultStore = ApplicationHelper.SaleDataFromDefaultStore ? generalRepository_1_1.StoresGet(true).First(t => t.IsDefault) : generalRepository_1_1.StoresGet(true).First(t => t.IsDefault); //TODO поиск ближайшего магазина
                            List<TaxRuleDetail> taxRules = generalServiceOld.TaxRulesGet(true).Where(t => t.TaxClass_ID == defaultStore.TaxClass_ID).ToList();
                            foreach (long sku_id in discountSKUids)
                            {
                                CartItem line = cp.ProductLines.FirstOrDefault(t => t.Package.ID == 0 && t.SKU.IdTitle.ID == sku_id);
                                if (line == null)
                                {
                                    continue;
                                }
                                line.Coupon_ID = discount.ID;
                                line.CouponType_ID = discount.Type_ID;
                                line.CouponDiscount = discount.IsPercent ? ((line.TotalPrice - line.TiersDiscount / line.TotalQty) * discount.Value / 100).GetPrice() : Math.Min(line.TotalPrice - line.TiersDiscount, discount.Value * line.TotalQty);

                                long country_id = line.Product.TaxByShipping ? cp.AddressShipping.Country_ID : cp.AddressBilling.Country_ID;
                                long state_id = line.Product.TaxByShipping ? cp.AddressShipping.State_ID : cp.AddressBilling.State_ID;
                                string zip = line.Product.TaxByShipping ? cp.AddressShipping.Zip : cp.AddressBilling.Zip;
                                List<TaxRuleDetail> tr = taxRules.Where(t => t.TaxShipping == line.Product.TaxByShipping && t.DestinationState_ID == state_id && t.DestinationCountry_ID == country_id).ToList();
                                TaxRuleDetail taxRule = tr.FirstOrDefault(t => (t.DestinationZip ?? string.Empty).ToLower().Trim() == zip.ToLower().Trim()) ?? tr.FirstOrDefault(t => string.IsNullOrEmpty(t.DestinationZip));
                                if (taxRule == null) continue;
                                decimal taxValue = (line.TotalPrice - line.Discount - line.CouponDiscount - line.TiersDiscount) * taxRule.SalesTax * (decimal)0.01;
                                taxServiceValue += taxValue;
                                lines.Remove(line);
                            }
                            foreach (CartItem line in lines)
                            {
                                long country_id = line.Product.TaxByShipping ? cp.AddressShipping.Country_ID : cp.AddressBilling.Country_ID;
                                long state_id = line.Product.TaxByShipping ? cp.AddressShipping.State_ID : cp.AddressBilling.State_ID;
                                string zip = line.Product.TaxByShipping ? cp.AddressShipping.Zip : cp.AddressBilling.Zip;
                                List<TaxRuleDetail> tr = taxRules.Where(t => t.TaxShipping == line.Product.TaxByShipping && t.DestinationState_ID == state_id && t.DestinationCountry_ID == country_id).ToList();
                                TaxRuleDetail taxRule = tr.FirstOrDefault(t => (t.DestinationZip ?? string.Empty).ToLower().Trim() == zip.ToLower().Trim()) ?? tr.FirstOrDefault(t => string.IsNullOrEmpty(t.DestinationZip));
                                if (taxRule == null) continue;
                                decimal taxValue = (line.TotalPrice - line.Discount - line.CouponDiscount - line.TiersDiscount) * taxRule.SalesTax * (decimal)0.01;
                                taxServiceValue += taxValue;
                            }
                            OrderService tax = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.Tax);
                            if (tax == null)
                            {
                                cp.Services.Add(new OrderService { IdTitle = new IdTitle { ID = (long)StoreServices.Tax, Title = StoreServices.Tax.ToString() }, Value = taxServiceValue.GetPrice(), IsAddInInvoice = true, Adding = true });
                            }
                            else
                            {
                                tax.Value = taxServiceValue.GetPrice();
                            }
                            break;
                        }
                    case DiscountType.AssignedToShipping:
                        {
                            dscType = "Shipping";
                            OrderService shipping = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.Shipping);
                            if (shipping == null) { return null; }
                            decimal couponValue = discount.IsPercent ? (shipping.Value * discount.Value / 100).GetPrice() : Math.Min(shipping.Value, discount.Value);
                            OrderService shippingCoupon = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.ShippingDiscount);
                            cp.ProductLines.ForEach(c => c.Coupon_ID = discount.ID);
                            if (shippingCoupon == null)
                            {
                                cp.Services.Add(new OrderService { IdTitle = new IdTitle { ID = (long)StoreServices.ShippingDiscount, Title = AppHelper.SplitCamelCase(StoreServices.ShippingDiscount.ToString()) }, IsAddInInvoice = true, Value = couponValue, Adding = false });
                            }
                            else
                            {
                                shippingCoupon.Value = couponValue;
                            }
                            break;
                        }
                    default: //order Subtotal
                        {
                            dscType = "Order subtotal";
                            decimal couponValue = discount.IsPercent ? (cp.ProductLines.Sum(l => l.TotalPrice - l.TiersDiscount) * discount.Value / 100).GetPrice() : Math.Min(cp.ProductLines.Sum(l => l.TotalPrice - l.TiersDiscount), discount.Value);
                            OrderService coupon = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.Coupon);
                            if (coupon == null)
                            {
                                cp.Services.Add(new OrderService { IdTitle = new IdTitle { ID = (long)StoreServices.Coupon, Title = StoreServices.Coupon.ToString() }, IsAddInInvoice = true, Value = couponValue, Adding = false });
                            }
                            else
                            {
                                coupon.Value = couponValue;
                            }
                            cp.ProductLines.ForEach(c => c.Coupon_ID = discount.ID);
                            StoreForm defaultStore = ApplicationHelper.SaleDataFromDefaultStore ? generalRepository_1_1.StoresGet(true).First(t => t.IsDefault) : generalRepository_1_1.StoresGet(true).First(t => t.IsDefault); //TODO поиск ближайшего магазина
                            List<TaxRuleDetail> taxRules = generalServiceOld.TaxRulesGet(true).Where(t => t.TaxClass_ID == defaultStore.TaxClass_ID).ToList();
                            foreach (CartItem line in cp.ProductLines)
                            {
                                decimal d = 0;
                                if (discount.IsPercent)
                                {
                                    d = ((line.TotalPrice - line.TiersDiscount / line.TotalQty) * discount.Value / 100).GetPrice();
                                }
                                else
                                {
                                    d = Math.Min(couponValue, line.TotalPrice);
                                    couponValue -= d;
                                }
                                if (!line.Product.IsTaxable) continue;
                                long country_id = line.Product.TaxByShipping ? cp.AddressShipping.Country_ID : cp.AddressBilling.Country_ID;
                                long state_id = line.Product.TaxByShipping ? cp.AddressShipping.State_ID : cp.AddressBilling.State_ID;
                                string zip = line.Product.TaxByShipping ? cp.AddressShipping.Zip : cp.AddressBilling.Zip;
                                List<TaxRuleDetail> tr = taxRules.Where(t => t.TaxShipping == line.Product.TaxByShipping && t.DestinationState_ID == state_id && t.DestinationCountry_ID == country_id).ToList();
                                TaxRuleDetail taxRule = tr.FirstOrDefault(t => (t.DestinationZip ?? string.Empty).ToLower().Trim() == zip.ToLower().Trim()) ?? tr.FirstOrDefault(t => string.IsNullOrEmpty(t.DestinationZip));
                                if (taxRule == null) continue;
                                decimal taxValue = (line.TotalPrice - line.Discount - d - line.TiersDiscount) * taxRule.SalesTax * (decimal)0.01;
                                taxServiceValue += taxValue;
                            }
                            OrderService tax = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.Tax);
                            if (tax == null)
                            {
                                cp.Services.Add(new OrderService { IdTitle = new IdTitle { ID = (long)StoreServices.Tax, Title = StoreServices.Tax.ToString() }, Value = taxServiceValue.GetPrice(), IsAddInInvoice = true, Adding = true });
                            }
                            else
                            {
                                tax.Value = taxServiceValue.GetPrice();
                            }
                            break;
                        }

                }
                Session["CartPayment"] = cp;
                OrderService taxService = cp.Services.FirstOrDefault(s => s.IdTitle.ID == (long)StoreServices.Tax) ?? new OrderService { Value = 0 };
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, string.Format("The {0} has been assigned to {1}.", discount.DiscountValueText, dscType), new { discount = cp.ProductsDiscount.GetCurrency(false), tax = taxService.Value.GetCurrency(false), total = cp.TotalDue.GetCurrency(false) }));
            }
            catch (Exception ex)
            {
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message));
            }
        }

        ////CalculaterDiscountForInvoices
        //private void CalculaterDiscountForInvoices(List<InvoiceDetail> invoices, decimal discountAmount)
        //{
        //  if (invoices.Count() == 0) return;
        //  decimal res = discountAmount / invoices.Count;
        //  if (res <= invoices.FirstOrDefault().TotalCost)
        //  {
        //    invoices.ForEach(I => I.TotalOrderDiscount = res);
        //    return;
        //  }
        //  invoices.First().TotalOrderDiscount = invoices.First().TotalCost;
        //  CalculaterDiscountForInvoices(invoices.Skip(1).ToList(), discountAmount - invoices.First().TotalCost);
        //}

        //if (discount != null)
        //  {
        //    pc.CouponDiscountValue = discount.DiscountValueText;
        //    pc.DiscountAssignedType = (Consts.DiscountAssignedType)discount.Type_ID;
        //    pc.DiscountCoupon_ID = discount.ID;
        //    switch (pc.DiscountAssignedType)
        //    {
        //      case Consts.DiscountAssignedType.Shipping:
        //        invoices.ForEach(I => I.RealSh = I.Shipping);
        //        invoices.ForEach(I => I.Shipping = discount.IsPercent ? Math.Max(I.Shipping * (1 - discount.DiscountValue / 100), 0) : Math.Max(I.Shipping - discount.DiscountValue, 0));
        //        break;
        //      case Consts.DiscountAssignedType.BuyerPremium:
        //        invoices.ForEach(I => I.RealBP = I.BuyerPremium);
        //        invoices.ForEach(I => I.BuyerPremium = discount.IsPercent ? Math.Max(I.BuyerPremium * (1 - discount.DiscountValue / 100), 0) : Math.Max(I.BuyerPremium - discount.DiscountValue, 0));
        //        break;
        //      default:
        //        invoices = invoices.OrderBy(i => i.TotalCost).ToList();
        //        invoices.ForEach(I => I.RealDiscount = I.Discount);
        //        if (discount.IsPercent)
        //          invoices.ForEach(I => I.TotalOrderDiscount = I.TotalCost * discount.DiscountValue / 100);
        //        else
        //          CalculaterDiscountForInvoices(invoices, discount.DiscountValue);
        //        invoices.ForEach(I => I.Discount += I.TotalOrderDiscount.GetValueOrDefault(0));
        //        break;
        //    }
        //    invoices.ForEach(I => I.Discount_ID = discount.ID);
        //  }

        //DiscardDiscount
        [HttpPost, Compress]
        public JsonResult DiscardDiscount()
        {
            try
            {
                CartPayment cp = Session["CartPayment"] as CartPayment;
                if (cp == null) throw new Exception("Checkout Failed. CartPayment is null.");
                List<OrderService> coupons = cp.Services.Where(s => s.IdTitle.ID == (long)StoreServices.Coupon || s.IdTitle.ID == (long)StoreServices.ShippingDiscount).ToList();
                OrderService taxService = cp.Services.SingleOrDefault(s => s.IdTitle.ID == (long)StoreServices.Tax);
                if (coupons.Any(t => t.IdTitle.ID == (long)StoreServices.Coupon))
                {
                    StoreForm defaultStore = ApplicationHelper.SaleDataFromDefaultStore ? generalRepository_1_1.StoresGet(true).First(t => t.IsDefault) : generalRepository_1_1.StoresGet(true).First(t => t.IsDefault); //TODO поиск ближайшего магазина
                    List<TaxRuleDetail> taxRules = generalServiceOld.TaxRulesGet(true).Where(t => t.TaxClass_ID == defaultStore.TaxClass_ID).ToList();
                    decimal taxServiceValue = 0;
                    foreach (CartItem line in cp.ProductLines)
                    {
                        line.Coupon_ID = null;
                        line.CouponType_ID = null;
                        line.CouponDiscount = 0;
                        long country_id = line.Product.TaxByShipping ? cp.AddressShipping.Country_ID : cp.AddressBilling.Country_ID;
                        long state_id = line.Product.TaxByShipping ? cp.AddressShipping.State_ID : cp.AddressBilling.State_ID;
                        string zip = line.Product.TaxByShipping ? cp.AddressShipping.Zip : cp.AddressBilling.Zip;
                        List<TaxRuleDetail> tr = taxRules.Where(t => t.TaxShipping == line.Product.TaxByShipping && t.DestinationState_ID == state_id && t.DestinationCountry_ID == country_id).ToList();
                        TaxRuleDetail taxRule = tr.FirstOrDefault(t => (t.DestinationZip ?? string.Empty).ToLower().Trim() == zip.ToLower().Trim()) ?? tr.FirstOrDefault(t => string.IsNullOrEmpty(t.DestinationZip));
                        if (taxRule == null) continue;
                        decimal taxValue = (line.TotalPrice - line.Discount - line.CouponDiscount - line.TiersDiscount) * taxRule.SalesTax * (decimal)0.01;
                        taxServiceValue += taxValue;
                    }
                    if (taxService == null)
                    {
                        cp.Services.Add(new OrderService { IdTitle = new IdTitle { ID = (long)StoreServices.Tax, Title = StoreServices.Tax.ToString() }, IsAddInInvoice = true, Value = taxServiceValue, Adding = true });
                    }
                    else
                    {
                        taxService.Value = taxServiceValue;
                    }
                }
                foreach (OrderService coupon in coupons)
                {
                    cp.Services.Remove(coupon);
                }
                Session["CartPayment"] = cp;
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "", new { tax = (taxService != null ? taxService.Value : 0).GetCurrency(false), total = cp.TotalDue.GetCurrency(false) }));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, "Checkout Failed. Please reload the page."));
            }
        }

        //ApplyGiftCard
        [HttpPost, Compress]
        public JsonResult ApplyGiftCard(string secureCode, short pinCode, decimal amount)
        {
            CartPayment cp = Session["CartPayment"] as CartPayment;
            if (cp == null) return JSON(null);
            GiftCard giftCard = productService.GiftCardsGet(true).FirstOrDefault(t => t.SecureCode == secureCode && t.PinCode == pinCode && (t.Status_ID == (long)GiftCardStatuses.Checked || t.Status_ID == (long)GiftCardStatuses.Pending));
            if (giftCard == null) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, "Gift Card does not exist."));
            if (cp.GiftCardPayments.Any(t => t.ID == giftCard.ID)) return JSON(new JsonExecuteResult(JsonExecuteResultTypes.ERROR, "This gift card already add to order."));
            CartPaymentGiftCard gc = new CartPaymentGiftCard { ID = giftCard.ID, Amount = Math.Min(amount, giftCard.RemainingAmount), IsValid = true, RemainingAmount = giftCard.RemainingAmount, Error = string.Empty, SecureCode = giftCard.SecureCode, PinCode = giftCard.PinCode };
            cp.GiftCardPayments.Add(gc);
            return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, new { alertMessage = giftCard.RemainingAmount < amount ? "Unfortunately, the amount you want is not available. The amount was changed to the available maximum." : null, card = new object[] { new { card_id = gc.ID, card_name = string.Format("{0}-{1}", gc.SecureCode, gc.PinCode), card_remaining = gc.RemainingAmount.GetCurrency(false), amount = gc.Amount.GetCurrency(false) } } }));
        }

        //DiscardGiftCard
        [HttpPost, Compress]
        public JsonResult DiscardGiftCard(long id)
        {
            CartPayment cp = Session["CartPayment"] as CartPayment;
            if (cp == null) return JSON(null);
            cp.GiftCardPayments.RemoveAll(t => t.ID == id);
            return JSON(new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, cp.GiftCardPayments.Any()));
        }
        #endregion
    }
}
