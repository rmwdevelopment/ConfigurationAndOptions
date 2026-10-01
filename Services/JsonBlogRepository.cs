using MicroBlog.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroBlog.Services
{
    public class JsonBlogRepository : IBlogRepository
    {

        //Create variables
        private readonly string _dataDir;
        private readonly string _dataFile;
        private readonly object _lock = new();

        //Json Options
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        //List an Id
        private List<Post> _posts = new();
        private int _nextId = 1;

        //Constructor
        public JsonBlogRepository(IWebHostEnvironment env)
        {
            _dataDir = Path.Combine(env.ContentRootPath, "data");
            _dataFile = Path.Combine(_dataDir, "posts.json");
            Directory.CreateDirectory(_dataDir);
            LoadFromDisk();
        }


        //Get All Method
        public IEnumerable<Post> GetAll()
        {
            lock (_lock) return _posts.OrderByDescending(p => p.CreatedUtc).ToList();
        }

        //Get By Id Method
        public Post? GetById(int id)
        {
            lock (_lock) return _posts.FirstOrDefault(p => p.Id == id);
        }

        //Add Method
        public void Add(Post post)
        {
            lock (_lock)
            {
                post.Id = _nextId++;
                post.CreatedUtc = DateTime.UtcNow;
                _posts.Add(post);
                SaveToDisk(); //save on each write
            }
        }

        //Save Method
        public void Save()
        {
            lock (_lock) SaveToDisk();
        }

        //Load from Disk
        private void LoadFromDisk()
        {
            if(!File.Exists(_dataFile)) { _posts = new(); _nextId = 1; return; }

            try
            {
                var text = File.ReadAllText(_dataFile);
                _posts = JsonSerializer.Deserialize<List<Post>>(text, _json) ?? new();
                _nextId = _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
            }
            catch
            {
                _posts = new();
                _nextId = 1;
            }
        }

        //Save to Disk
        private void SaveToDisk()
        {
            var text = JsonSerializer.Serialize(_posts, _json);
            File.WriteAllText(text, _dataDir);
        }
    }
}
