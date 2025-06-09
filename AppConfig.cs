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
        
        // 公共无参构造函数，用于JSON反序列化
        public AppConfig() { }
        
        // 加载配置
        private static AppConfig Load()
        {
            Logger.Log($"尝试加载配置文件: {ConfigFilePath}");
            
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    Logger.Log($"读取到配置内容: {json}");
                    
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            // 使用简单的字典方式解析JSON，避免反序列化问题
                            var jsonDoc = JsonDocument.Parse(json);
                            var root = jsonDoc.RootElement;
                            
                            var config = new AppConfig();
                            
                            // 手动提取属性
                            if (root.TryGetProperty("WordWrap", out var wordWrapProp) && wordWrapProp.ValueKind == JsonValueKind.True)
                                config.WordWrap = true;
                                
                            if (root.TryGetProperty("FontFamily", out var fontFamilyProp) && fontFamilyProp.ValueKind == JsonValueKind.String)
                                config.FontFamily = fontFamilyProp.GetString() ?? "Consolas";
                                
                            if (root.TryGetProperty("FontSize", out var fontSizeProp) && fontSizeProp.ValueKind == JsonValueKind.Number)
                                config.FontSize = fontSizeProp.GetSingle();
                                
                            // 提取最近文件列表
                            if (root.TryGetProperty("RecentFiles", out var recentFilesProp) && recentFilesProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in recentFilesProp.EnumerateArray())
                                {
                                    if (item.ValueKind == JsonValueKind.String)
                                    {
                                        string? path = item.GetString();
                                        if (!string.IsNullOrEmpty(path))
                                            config.RecentFiles.Add(path);
                                    }
                                }
                            }
                            
                            // 提取上次打开的标签页
                            if (root.TryGetProperty("LastOpenedTabs", out var lastOpenedTabsProp) && lastOpenedTabsProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in lastOpenedTabsProp.EnumerateArray())
                                {
                                    if (item.ValueKind == JsonValueKind.String)
                                    {
                                        string? path = item.GetString();
                                        if (!string.IsNullOrEmpty(path))
                                            config.LastOpenedTabs.Add(path);
                                    }
                                }
                            }
                            
                            Logger.Log($"配置加载成功: WordWrap={config.WordWrap}, FontFamily={config.FontFamily}, LastOpenedTabs.Count={config.LastOpenedTabs.Count}");
                            return config;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"JSON解析失败: {ex.Message}");
                        }
                    }
                }
                
                Logger.Log("配置文件不存在或为空，使用默认配置");
            }
            catch (Exception ex)
            {
                // 加载失败时记录错误，但继续使用默认配置
                Logger.Log($"加载配置失败: {ex.Message}");
                Logger.Log($"异常堆栈: {ex.StackTrace}");
            }
            
            // 返回默认配置
            var defaultConfig = new AppConfig();
            Logger.Log($"使用默认配置: WordWrap={defaultConfig.WordWrap}");
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
                
                Logger.Log($"配置已保存到: {ConfigFilePath}");
                Logger.Log($"配置内容: WordWrap={WordWrap}, FontFamily={FontFamily}, LastOpenedTabs.Count={LastOpenedTabs.Count}");
            }
            catch (Exception ex)
            {
                Logger.Log($"保存配置失败: {ex.Message}");
                Logger.Log($"异常堆栈: {ex.StackTrace}");
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