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
        public bool ShowLineNumbers { get; set; } = false; // 默认不显示行号
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
            System.Diagnostics.Debug.WriteLine($"尝试加载配置文件: {ConfigFilePath}");
            
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    System.Diagnostics.Debug.WriteLine($"读取到配置内容: {json}");
                    
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new JsonSerializerOptions { 
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip
                        };
                        
                        var config = JsonSerializer.Deserialize<AppConfig>(json, options);
                        if (config != null)
                        {
                            // 确保配置中的所有属性都被正确加载
                            // 如果配置文件中缺少某些属性，使用默认值
                            if (string.IsNullOrEmpty(config.FontFamily))
                                config.FontFamily = "Consolas";
                            if (config.FontSize <= 0)
                                config.FontSize = 10;
                            if (config.RecentFiles == null)
                                config.RecentFiles = new List<string>();
                            if (config.LastOpenedTabs == null)
                                config.LastOpenedTabs = new List<string>();
                                
                            System.Diagnostics.Debug.WriteLine($"配置加载成功: WordWrap={config.WordWrap}, ShowLineNumbers={config.ShowLineNumbers}, FontFamily={config.FontFamily}, LastOpenedTabs.Count={config.LastOpenedTabs.Count}");
                            return config;
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("配置文件不存在或为空，使用默认配置");
            }
            catch (Exception ex)
            {
                // 加载失败时记录错误，但继续使用默认配置
                System.Diagnostics.Debug.WriteLine($"加载配置失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"异常堆栈: {ex.StackTrace}");
            }
            
            // 返回默认配置
            var defaultConfig = new AppConfig();
            System.Diagnostics.Debug.WriteLine($"使用默认配置: WordWrap={defaultConfig.WordWrap}, ShowLineNumbers={defaultConfig.ShowLineNumbers}");
            return defaultConfig;
        }
        
        // 保存配置
        public void Save()
        {
            try
            {
                // 确保配置目录存在
                string configDir = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // 确保集合已初始化
                if (RecentFiles == null) RecentFiles = new List<string>();
                if (LastOpenedTabs == null) LastOpenedTabs = new List<string>();
                
                // 序列化配置
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                
                // 写入文件
                File.WriteAllText(ConfigFilePath, json);
                
                System.Diagnostics.Debug.WriteLine($"配置已保存到: {ConfigFilePath}");
                System.Diagnostics.Debug.WriteLine($"配置内容: WordWrap={WordWrap}, ShowLineNumbers={ShowLineNumbers}, FontFamily={FontFamily}, LastOpenedTabs.Count={LastOpenedTabs.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"异常堆栈: {ex.StackTrace}");
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