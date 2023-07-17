using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Helpers;
using FreeCourse.Web.Models.Catalogs;
using FreeCourse.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient httpClient;
        private readonly IPhotoStockService photoStockService;
        private readonly PhotoHelper photoHelper;

        public CatalogService(HttpClient httpClient, IPhotoStockService photoStockService, PhotoHelper photoHelper)
        {
            this.httpClient = httpClient;
            this.photoStockService = photoStockService;
            this.photoHelper = photoHelper;
        }

        public async Task<bool> CreateCourseAsync(CourseCreateInput courseCreateInput)
        {
            var resultPhotoService = await this.photoStockService.UploadPhoto(courseCreateInput.PhotoFormFile);

            if (resultPhotoService != null)
                courseCreateInput.Picture = resultPhotoService.Url;

            var response = await this.httpClient.PostAsJsonAsync<CourseCreateInput>("courses", courseCreateInput);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseAsync(string courseId)
        {
            var response = await this.httpClient.DeleteAsync($"courses/{courseId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoryAsync()
        {
            //http://localhost:5000/services/catalog/categories
            var response = await this.httpClient.GetAsync("categories");

            if (response is { IsSuccessStatusCode: false })
                return null;

            //elde etdiyimiz data ni json formatda oxuya bilmek ucun.
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<List<CategoryViewModel>>>();
            return responseSuccess.Data;
        }

        public async Task<List<CourseViewModel>> GetAllCourseAsync()
        {
            //http://localhost:5000/services/catalog/courses
            var response = await this.httpClient.GetAsync("courses");

            if (response is { IsSuccessStatusCode: false })
                return null;

            //elde etdiyimiz data ni json formatda oxuya bilmek ucun.
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<List<CourseViewModel>>>();

            responseSuccess.Data.ForEach(x =>
            {
                //x.picture elave edilen data => http://localhost:5012/services/photostock/x.picture
                x.Picture = this.photoHelper.GetPhotoStockUrl(x.Picture);
            });

            return responseSuccess.Data;
        }

        public async Task<List<CourseViewModel>> GetAllCourseByUserIdAsync(string userId)
        {
            var response = await this.httpClient.GetAsync($"courses/GetAllByUserId/{userId}");

            if (response is { IsSuccessStatusCode: false })
                return null;

            //elde etdiyimiz data ni json formatda oxuya bilmek ucun.
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<List<CourseViewModel>>>();
            responseSuccess.Data.ForEach(x =>
            {
                x.Picture = this.photoHelper.GetPhotoStockUrl(x.Picture);
            });

            return responseSuccess.Data;
        }

        public async Task<CourseViewModel> GetByCourseId(string courseId)
        {
            var response = await this.httpClient.GetAsync($"courses/{courseId}");

            if (response is { IsSuccessStatusCode: false })
                return null;

            //elde etdiyimiz data ni json formatda oxuya bilmek ucun.
            var responseSuccess = await response.Content.ReadFromJsonAsync<Response<CourseViewModel>>();
            return responseSuccess.Data;
        }

        public async Task<bool> UpdateCourseAsync(CourseUpdateInput courseUpdateInput)
        {
            var response = await this.httpClient.PutAsJsonAsync<CourseUpdateInput>("courses", courseUpdateInput);
            return response.IsSuccessStatusCode;
        }
    }
}