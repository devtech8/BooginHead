using System.Web.Mvc;
using Nop.Web.Framework.Security;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BaseNopController
    {
        //default landing page - MG
        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Index()
        {
            return View();
        }

        //Product Categories Page - MG
        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult ProductCats()
        {
            return View();
        }

        //[ChildActionOnly]
        //public ActionResult ProductCats()
        //{
        //    var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage()
        //        .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
        //        .ToList();

        //    var listModel = categories
        //        .Select(x =>
        //        {
        //            var catModel = x.ToModel();

        //            //prepare picture model
        //            int pictureSize = _mediaSettings.CategoryThumbPictureSize;
        //            var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
        //            catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
        //            {
        //                var pictureModel = new PictureModel()
        //                {
        //                    FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId),
        //                    ImageUrl = _pictureService.GetPictureUrl(x.PictureId, pictureSize),
        //                    Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
        //                    AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
        //                };
        //                return pictureModel;
        //            });

        //            return catModel;
        //        })
        //        .ToList();

        //    return PartialView(listModel);
        //}


    }
}
