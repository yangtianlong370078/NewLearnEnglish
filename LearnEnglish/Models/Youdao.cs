using System.Net;

namespace LearnEnglish.Models
{
    public class Youdao
    {
        private int _type; // 发音方式
        private string _word; // 单词
        private string _dirRoot; // 文件根目录
        private string _dirSpeech; // 当前发音库目录
        private string _fileName; // MP3文件名
        private string _filePath; // MP3文件路径
        private string _url;
        public Youdao(int type = 0, string word = "hellow")
        {
            // 小写
            _word = word.ToLowerInvariant();

            _type = type; // 发音方式

            // 文件根目录
            _dirRoot = Path.GetDirectoryName(Path.GetFullPath(typeof(Youdao).Assembly.Location));

            SetAccent(type);

            // 创建语音库目录（如果不存在）
            CreateDirectoryIfNotExists("Speech_US");
            CreateDirectoryIfNotExists("Speech_EN");
        }

        public void SetAccent(int type = 0)
        {
            _type = type; // 发音方式
            SetAccentImpl(type);
        }

        public int GetAccent()
        {
            return _type;
        }

        public string Download(string word)
        {
            word = word.ToLowerInvariant(); // 小写
            string filePath = _getWordMp3FilePath(word);

            if (filePath == null)
            {
                _getURL();
                Console.WriteLine($"正在下载 {_word}.mp3 ...");
                DownloadFile(_url, _filePath);
                Console.WriteLine($"{_word}.mp3 下载完成");
            }
            else
            {
                Console.WriteLine($"已存在 {_word}.mp3，无需下载");
            }

            return _filePath;
        }

        private void SetAccentImpl(int type)
        {
            if (type == 0)
            {
                _dirSpeech = Path.Combine(_dirRoot, "Speech_US"); // 美音库
            }
            else
            {
                _dirSpeech = Path.Combine(_dirRoot, "Speech_EN"); // 英音库
            }
        }

        private void CreateDirectoryIfNotExists(string dirName)
        {
            string dirPath = Path.Combine(_dirRoot, dirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        private string _getWordMp3FilePath(string word)
        {
            _word = word;
            _fileName = $"{_word}.mp3";
            _filePath = Path.Combine(_dirSpeech, _fileName);

            return File.Exists(_filePath) ? _filePath : null;
        }

        private void _getURL()
        {
            _url = $"http://dict.youdao.com/dictvoice?type={_type}&audio={_word}";
        }

        private void DownloadFile(string url, string destination)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, destination);
            }
        }

        //static void Main(string[] args)
        //{
        //    var sp = new Youdao();
        //    string filePath = sp.Download("word");
        //    Console.WriteLine($"下载后的文件路径: {filePath}");
        //}
    }
}
