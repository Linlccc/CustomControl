using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProtoBuf;
using GDKSerialize;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImagesListC
{
    public class ImageLS
    {
        static string path = Application.StartupPath + "\\ImgSave\\";//文件位置
        static string desStr = "zxcvzxcv";//密码
        static ImageLS()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 图片组集合
        /// </summary>
        public static List<ImageListr> ImageListS = new List<ImageListr>();


        /// <summary>
        /// 保存图片文件文件
        /// </summary>
        public static void EmojiSave()
        {
            SerializeCls.Serialize(ImageLS.ImageListS, path + "Emoji.dat", desStr);
        }
        /// <summary>
        /// 获取图片文件
        /// </summary>
        /// <returns></returns>
        public static void EmojiLoad(Form f)
        {
            ImageLS.ImageListS = SerializeCls.Deserialize<List<ImageListr>>(path + "Emoji.dat", desStr);
            if (ImageLS.ImageListS != null) return;
            ImageLS.ImageListS = new List<ImageListr>();
        }

        /// <summary>
        /// 切割图片
        /// </summary>
        /// <param name="oImg">原图</param>
        /// <param name="cutImgWidth">切割的宽</param>
        /// <param name="cutImgHeigth">切割的高</param>
        /// <param name="spacWidth">宽间距</param>
        /// <param name="spacHeigth">高间距</param>
        /// <param name="imgWidth">切割后的图片宽</param>
        /// <param name="imgHeigth">切割后的图片高</param>
        /// <returns></returns>
        public static Bitmap[] GetImageCut(Bitmap oImg, int cutImgWidth, int cutImgHeigth, int spacWidth, int spacHeigth, int imgWidth = 0, int imgHeigth = 0)
        {
            int rowNum = oImg.Height / (cutImgHeigth + spacWidth);//要切割的行数
            int colNum = oImg.Width / (cutImgWidth + spacWidth);//要切割的列数
            Bitmap[] bitmapArr = new Bitmap[rowNum * colNum];//图片集合

            if (imgWidth == 0)//如果切割后的大小为0，就等于切割图片的大小
            {
                imgWidth = cutImgWidth;
                imgHeigth = cutImgHeigth;
            }

            int srcImageX = spacWidth / 2;//图片切割位置的x轴
            int srcImageY = spacHeigth / 2;//图片切割位置的y轴

            for (int rowIdx = 0; rowIdx < rowNum; rowIdx++)
            {
                for (int colIdx = 0; colIdx < colNum; colIdx++)
                {
                    int curIdx = rowIdx * colNum + colIdx;//索引
                    bitmapArr[curIdx] = new Bitmap(imgWidth, imgHeigth);//实例出图片大小
                    Graphics newBmpGraphics = Graphics.FromImage(bitmapArr[curIdx]);//在该图片上创建一个绘制实例
                    Rectangle destImageRect = new Rectangle(0, 0, imgWidth, imgHeigth);//切割出来的图片缩放到这个大小
                    Rectangle srcImageRect = new Rectangle(srcImageX, srcImageY, cutImgWidth, cutImgHeigth);//从原始图片切割的位置
                    newBmpGraphics.DrawImage(oImg, destImageRect, srcImageRect, GraphicsUnit.Pixel);
                    srcImageX += cutImgWidth + spacWidth;
                }
                srcImageY += cutImgHeigth + spacHeigth;
                srcImageX = spacWidth / 2;
            }

            return bitmapArr;
        }
        /// <summary>
        /// 将Object类型对象转换为二进制序列字符串
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns></returns>
        public static string SerializeObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            string result = string.Empty;
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                byte[] byt = new byte[stream.Length];
                byt = stream.ToArray();
                //result = Encoding.UTF8.GetString(byt, 0, byt.Length);
                result = Convert.ToBase64String(byt);
                stream.Flush();
            }
            return result;
        }
        /// <summary>
        /// 将二进制序列字符串转换为Object类型对象
        /// </summary>
        /// <param name="str">要转换成对象的二进制字符串</param>
        /// <returns></returns>
        public static object DeserializeObject(string str)
        {
            IFormatter formatter = new BinaryFormatter();
            //byte[] byt = Encoding.UTF8.GetBytes(str);
            byte[] byt = Convert.FromBase64String(str);
            object obj = null;
            using (Stream stream = new MemoryStream(byt, 0, byt.Length))
            {
                obj = formatter.Deserialize(stream);
            }
            return obj;
        }
    }
    /// <summary>
    /// 图片集合信息
    /// </summary>
    [ProtoContract]
    public class ImageListr
    {
        public ImageListr() { }
        /// <summary>
        /// 图片集合信息
        /// </summary>
        /// <param name="name">集合名称</param>
        /// <param name="image">集合头图片</param>
        public ImageListr(string name, Image image)
        {
            this.Name = name;
            this.Imager = image;
        }
        /// <summary>
        /// 这组图片的名称
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }
        /// <summary>
        /// 标题图片的二进制字符
        /// </summary>
        [ProtoMember(2)]
        private string ImageString { get; set; }

        private Image imager;
        /// <summary>
        /// 标题图片
        /// </summary>
        public Image Imager
        {
            get
            {
                if (imager == null)
                {
                    if (ImageString.Length < 1)
                        return null;
                    imager = (Bitmap)ImageLS.DeserializeObject(ImageString);
                }
                return imager;
            }
            set
            {
                if (imager == value) return;
                imager = value;
                ImageString = ImageLS.SerializeObject(imager);
            }
        }
        /// <summary>
        /// 图片集合
        /// </summary>
        [ProtoMember(3)]
        public List<ImageAndValue> Images { get; set; } = new List<ImageAndValue>();
    }
    /// <summary>
    /// 图片集合
    /// </summary>
    [ProtoContract]
    public class ImageAndValue
    {
        public ImageAndValue() { }
        public ImageAndValue(string value, Image img)
        {
            this.Value = value;
            this.Image = img;
        }
        /// <summary>
        /// 图片对应的值
        /// </summary>
        [ProtoMember(1)]
        public string Value { get; set; }
        /// <summary>
        /// 图片的二进制字符
        /// </summary>
        [ProtoMember(2)]
        private string ImageString { get; set; }
        private Image image;
        /// <summary>
        /// 图片
        /// </summary>
        public Image Image
        {
            get
            {
                if (image == null)
                {
                    if (ImageString.Length < 1)
                        return null;
                    image = (Bitmap)ImageLS.DeserializeObject(ImageString);
                }
                return image;
            }
            set
            {
                if (image == value) return;
                image = value;
                ImageString = ImageLS.SerializeObject(image);
            }
        }
    }
}
