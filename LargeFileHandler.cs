using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WawaEditor
{
    /// <summary>
    /// 大文件处理类，实现行索引和虚拟化视图
    /// </summary>
    public class LargeFileHandler
    {
        private string _filePath;
        private List<long> _lineOffsets = new List<long>();
        private FileStream _fileStream;
        private Dictionary<int, string> _changeLog = new Dictionary<int, string>();
        private const int BUFFER_SIZE = 64 * 1024; // 64KB 缓冲区
        private const int CACHE_SIZE = 1000; // 缓存前后各1000行
        
        // 缓存
        private Dictionary<int, string> _cachedLines = new Dictionary<int, string>();
        private int _cacheStartLine = -1;
        private int _cacheEndLine = -1;
        
        public int TotalLines => _lineOffsets.Count;
        public bool IsModified => _changeLog.Count > 0;
        
        public LargeFileHandler(string filePath)
        {
            _filePath = filePath;
            BuildLineIndex();
        }
        
        /// <summary>
        /// 构建行索引
        /// </summary>
        private void BuildLineIndex()
        {
            Logger.Log($"开始构建行索引: {_filePath}");
            
            _lineOffsets.Clear();
            _lineOffsets.Add(0); // 第一行从0开始
            
            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                long filePosition = 0;
                int bytesRead;
                
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        // 检测换行符
                        if (buffer[i] == '\n')
                        {
                            _lineOffsets.Add(filePosition + i + 1);
                        }
                    }
                    
                    filePosition += bytesRead;
                }
            }
            
            Logger.Log($"行索引构建完成，共 {_lineOffsets.Count} 行");
            
            // 打开文件流以供后续读取
            _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        
        /// <summary>
        /// 获取指定行的内容
        /// </summary>
        public string GetLine(int lineNumber)
        {
            if (lineNumber < 0 || lineNumber >= _lineOffsets.Count)
                return string.Empty;
                
            // 检查是否有修改
            if (_changeLog.TryGetValue(lineNumber, out string modifiedLine))
                return modifiedLine;
                
            // 检查是否在缓存中
            if (_cachedLines.TryGetValue(lineNumber, out string cachedLine))
                return cachedLine;
                
            // 如果不在缓存范围内，更新缓存
            if (lineNumber < _cacheStartLine || lineNumber > _cacheEndLine)
            {
                UpdateCache(lineNumber);
                
                // 再次检查缓存
                if (_cachedLines.TryGetValue(lineNumber, out cachedLine))
                    return cachedLine;
            }
            
            // 直接从文件读取单行
            return ReadLineFromFile(lineNumber);
        }
        
        /// <summary>
        /// 更新缓存
        /// </summary>
        private void UpdateCache(int centerLine)
        {
            _cachedLines.Clear();
            
            int startLine = Math.Max(0, centerLine - CACHE_SIZE);
            int endLine = Math.Min(_lineOffsets.Count - 1, centerLine + CACHE_SIZE);
            
            _cacheStartLine = startLine;
            _cacheEndLine = endLine;
            
            // 读取一批行到缓存
            for (int i = startLine; i <= endLine; i++)
            {
                // 如果有修改，使用修改后的内容
                if (_changeLog.TryGetValue(i, out string modifiedLine))
                {
                    _cachedLines[i] = modifiedLine;
                    continue;
                }
                
                // 否则从文件读取
                _cachedLines[i] = ReadLineFromFile(i);
            }
        }
        
        /// <summary>
        /// 从文件中读取指定行
        /// </summary>
        private string ReadLineFromFile(int lineNumber)
        {
            if (lineNumber < 0 || lineNumber >= _lineOffsets.Count)
                return string.Empty;
                
            long startOffset = _lineOffsets[lineNumber];
            long endOffset = (lineNumber < _lineOffsets.Count - 1) 
                ? _lineOffsets[lineNumber + 1] 
                : _fileStream.Length;
                
            int length = (int)(endOffset - startOffset);
            if (length <= 0) return string.Empty;
            
            byte[] buffer = new byte[length];
            
            lock (_fileStream)
            {
                _fileStream.Position = startOffset;
                _fileStream.Read(buffer, 0, length);
            }
            
            // 移除行尾的\r\n
            if (length > 0 && buffer[length - 1] == '\n')
                length--;
            if (length > 0 && buffer[length - 1] == '\r')
                length--;
                
            return Encoding.UTF8.GetString(buffer, 0, length);
        }
        
        /// <summary>
        /// 修改指定行的内容
        /// </summary>
        public void ModifyLine(int lineNumber, string newContent)
        {
            if (lineNumber < 0 || lineNumber >= _lineOffsets.Count)
                return;
                
            _changeLog[lineNumber] = newContent;
            
            // 更新缓存
            if (lineNumber >= _cacheStartLine && lineNumber <= _cacheEndLine)
            {
                _cachedLines[lineNumber] = newContent;
            }
        }
        
        /// <summary>
        /// 获取一批行用于显示
        /// </summary>
        public List<string> GetVisibleLines(int startLine, int count)
        {
            List<string> lines = new List<string>();
            
            // 确保范围有效
            startLine = Math.Max(0, startLine);
            int endLine = Math.Min(_lineOffsets.Count - 1, startLine + count - 1);
            
            // 如果不在缓存范围内，更新缓存
            if (startLine < _cacheStartLine || endLine > _cacheEndLine)
            {
                int centerLine = startLine + (count / 2);
                UpdateCache(centerLine);
            }
            
            // 获取行内容
            for (int i = startLine; i <= endLine; i++)
            {
                lines.Add(GetLine(i));
            }
            
            return lines;
        }
        
        /// <summary>
        /// 保存修改到文件
        /// </summary>
        public void SaveChanges()
        {
            if (!IsModified) return;
            
            string tempFilePath = _filePath + ".temp";
            
            try
            {
                using (var writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
                {
                    for (int i = 0; i < _lineOffsets.Count; i++)
                    {
                        string line;
                        
                        // 使用修改后的内容或原始内容
                        if (_changeLog.TryGetValue(i, out string modifiedLine))
                            line = modifiedLine;
                        else
                            line = ReadLineFromFile(i);
                            
                        writer.WriteLine(line);
                    }
                }
                
                // 关闭文件流
                _fileStream.Close();
                
                // 替换原文件
                File.Delete(_filePath);
                File.Move(tempFilePath, _filePath);
                
                // 重新打开文件并重建索引
                _changeLog.Clear();
                BuildLineIndex();
                
                Logger.Log($"文件保存成功: {_filePath}");
            }
            catch (Exception ex)
            {
                Logger.Log($"保存文件失败: {ex.Message}");
                
                // 重新打开文件
                if (_fileStream == null || !_fileStream.CanRead)
                {
                    _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                
                // 尝试删除临时文件
                try { File.Delete(tempFilePath); } catch { }
                
                throw;
            }
        }
        
        /// <summary>
        /// 关闭文件
        /// </summary>
        public void Close()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream = null;
            }
        }
    }
}