using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChunckStack
{
    class Chunk
    {
        public int _chunkSize;
        public int _fileSize;
        public string _fileName;
        string _id;
        string _directory;
        bool _checkTemp = true;
        bool _firstChunk = true;
        public Chunk(string id, string directory, string fileName, int chunkSize, int fileSize)
        {
            _directory = directory;
            _id = id;
            _fileName = fileName;
            _chunkSize = chunkSize;
            _fileSize = fileSize;
        }
        public void AppendToTemp(string data)
        {
            try
            {
                if (File.Exists(_directory + _id))
                {
                    if (_checkTemp)
                    {
                        File.Delete(_directory + _id);
                        File.Create(_directory + _id).Close();
                        _checkTemp = false;
                    }
                }
                else
                    _checkTemp = false;
                int dataLength = data.Length / 8;
                if (_firstChunk)
                {
                    int currentDataSize = _fileSize % _chunkSize;
                    if (currentDataSize == 0)
                        currentDataSize = _chunkSize;
                    if (dataLength != currentDataSize)
                        return;
                    _firstChunk = false;
                }
                else
                    if (dataLength != _chunkSize)
                        return;

                StreamWriter sw = File.AppendText(_directory + _id);
                sw.Write(data);
                sw.Close();
            }   
            catch (Exception e)
            {
               // MessageBox.Show(e.Message);
            }

        }
        public long Upload(string data)
        {
            AppendToTemp(data);
            FileInfo fInfo = new FileInfo(_directory + _id);
            return (fInfo.Length);
        }
        public string[] GetFileNames(string path, string filter)
        {
            string[] files = Directory.GetFiles((path == "") ? Directory.GetCurrentDirectory() : path,"*"+_fileName);
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }
        public string SaveData()
        {
            try
            {
                int carry = 1;
                if (File.Exists(_directory+_fileName))
                {
                    bool fileNameSwap = true;
                    try
                    {
                        System.IO.File.Move(_directory + _fileName, _directory + carry + "_" + _fileName);
                    }
                    catch
                    {
                        fileNameSwap = false;
                    }

                    if (fileNameSwap)
                        System.IO.File.Move(_directory + _id, _directory + _fileName);
                    else
                    {
                        string[] files = GetFileNames(_directory, _fileName);
                        while (true)
                        {
                            bool nameFound = true;
                            for (int i = 0; i < files.Length; i++)
                                if (files[i] == carry + "_" + _fileName)
                                {
                                    carry++;
                                    nameFound = false;
                                }
                            if (nameFound)
                                break;
                        }
                        System.IO.File.Move(_directory + _id, _directory + carry + "_" + _fileName);
                    }
                }
                else
                    System.IO.File.Move(_directory + _id, _directory + _fileName);
                return ("SUCCESS");
            }
            catch (Exception e)
            {
                return (e.Message);
            }
        }
        public string CancelUpload()
        {
            try
            {
                if (File.Exists(_directory + _id))
                    File.Delete(_directory + _id);
                return ("SUCCESS");
            }
            catch (Exception e)
            {
                return (e.Message);
            }
        }
        public string DataCheckSum(string clientChecksum)
        {
            try
            {
                string hash;
                using (var md5 = MD5.Create())
                    using (var stream = File.OpenRead(_directory + _id))
                        hash = System.Text.Encoding.Default.GetString(md5.ComputeHash(stream));

                //MD5 md5 = System.Security.Cryptography.MD5.Create();
                //long dataLength = (new FileInfo(_directory + _id)).Length/8;
                //byte[] dataBytes = new byte[dataLength];
                //string data = File.ReadAllText(_directory + _id);
                //for (int i = 0; i < dataLength; i++)
                //    dataBytes[i] = Convert.ToByte(data.Substring(8 * i, 8), 2);

                //byte[] hash = md5.ComputeHash(dataBytes);
                //StringBuilder sb = new StringBuilder();
                //for (int i = 0; i < hash.Length; i++)
                //    sb.Append(hash[i].ToString("X2"));//sb.Append(hash[i].ToString(“x2″)); LOWER CASE
                if (hash!=clientChecksum)
                    return ("NO_MATCH");
                return ("MATCH");
            }
            catch (Exception e)
            {
                return (e.Message);
            }
        }
    }
}
