using PhotoSauce.MagicScaler;

namespace Blog.Data.FileManager
{
    public class FileManager : IFileManager
    {
        private readonly string _imagePath;

        //public string ImagePath => _imagePath;


        public FileManager(IConfiguration config)
        {

            _imagePath = config["ConnectionStrings:Images"];
            //Console.WriteLine($"ImagePath from config: {_imagePath}");

        }

        public FileStream ImageStream(string image)
        {
            return new FileStream(Path.Combine(_imagePath, image), FileMode.Open, FileAccess.Read);
        }

        public bool RemoveImage(string image)
        {

            try
            {
                var file = Path.Combine(_imagePath, image);

                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }



        public async Task<string> SaveImage(IFormFile image)
        {
            try
            {
                //Internet Explorer Error   C:/User/BlaBlaBla/image.jpg
                //var fileName = image.FileName;


                var save_path = Path.Combine(_imagePath);

                if (!Directory.Exists(save_path))
                {
                    Directory.CreateDirectory(save_path);
                }




                var mime = image.FileName.Substring(image.FileName.LastIndexOf('.'));
                var fileName = $"img_{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}{mime}";

                //var filePath = Path.Combine(save_path, fileName);

                using (var fileStream = new FileStream(Path.Combine(save_path, fileName), FileMode.Create))
                {
                    //await image.CopyToAsync(fileStream);
                    MagicImageProcessor.ProcessImage(image.OpenReadStream(), fileStream, ImageOptions());
                }



                return fileName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Error";
            }
        }

        private ProcessImageSettings ImageOptions() => new ProcessImageSettings
        {
            Width = 800,
            Height = 500,
            ResizeMode = CropScaleMode.Crop, 
        };
    }
}
