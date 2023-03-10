using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        private int ChunkSize = 1024 * 1024;//1mb
        private int MaxConcurrentTasks = 3;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            UploadFile().ConfigureAwait(false);
            sw.Stop();
            MessageBox.Show($@"上传完成,耗时：{sw.ElapsedMilliseconds}");
        }

        private async Task UploadFile()
        {
            var filePath = txtFilePath.Text;
            var fileName = Path.GetFileName(filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, true);
            var fileId = Guid.NewGuid().ToString();
            var fileSize = fileStream.Length;

            var totalChunks = (int)Math.Ceiling(fileSize / (double)ChunkSize);

            // 使用Semaphore限制最大并发数
            var semaphore = new SemaphoreSlim(MaxConcurrentTasks);
            using (var client = new HttpClient())
            {
                var tasks = new List<Task>();
                for (int i = 0; i < totalChunks; i++)
                {
                    await semaphore.WaitAsync();

                    var chunkIndex = i;
                    var currentBlockSize = (i == totalChunks - 1) ? (int)(fileSize - (long)i * ChunkSize) : ChunkSize;

                    var buffer = new byte[currentBlockSize];
                    await fileStream.ReadAsync(buffer, 0, currentBlockSize);

                    var formData = new MultipartFormDataContent();
                    formData.Add(new ByteArrayContent(buffer), "file", fileName);
                    formData.Add(new StringContent(fileId), "fileId");
                    formData.Add(new StringContent(chunkIndex.ToString()), "chunkIndex");
                    formData.Add(new StringContent(totalChunks.ToString()), "totalChunks");

                    var url = new Uri(txtServerIP.Text + "api/upload");
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Content = formData;
                    var response = await client.SendAsync(requestMessage);
                    if (!response.IsSuccessStatusCode)
                    {
                        // 处理上传失败的情况
                    }

                    semaphore.Release();
                }
            }

            fileStream.Dispose(); // 释放文件流资源
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dialog.FileName;
            }
        }
    }
}
