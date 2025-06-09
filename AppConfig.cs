using System.Text.Json;

namespace WawaEditor
{
    public class AppConfig
    {
        // 配置文件路径
        private static readonly string ConfigFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "wawaeditor_config.json");
        
        // 单例实例
        private static AppConfig? _instance;
        
        // 最近打开的文件列表
        public List<string> RecentFiles { get; set; } = new List<string>();
        
        // 上次打开的标签页
        public List<string> LastOpenedTabs { get; set; } = new List<string>();
        
        // 其他配置项
        public bool WordWrap { get; set; } = false;
        public bool ShowLineNumbers { get; set; } = true;
        public string FontFamily { get; set; } = "Consolas";
        public float FontSize { get; set; } = 10;
        
        // 获取单例实例
        public static AppConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }
        
        // 私有构造函数，防止外部实例化
        private AppConfig() { }
        
        // 加载配置
        private static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                // 加载失败时记录错误，但继续使用默认配置
                System.Diagnostics.Debug.WriteLine($"加载配置失败: {ex.Message}");
            }
            
            // 返回默认配置
            return new AppConfig();
        }
        
        // 保存配置
        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
            }
        }
        
        // 添加最近打开的文件
        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;
                
            // 如果已存在，先移除
            RecentFiles.Remove(filePath);
            
            // 添加到列表开头
            RecentFiles.Insert(0, filePath);
            
            // 保持列表最多10项
            while (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
            
            // 保存配置
            Save();
        }
    }
}