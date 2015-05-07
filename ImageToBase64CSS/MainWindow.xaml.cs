using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;



namespace ImageToBase64CSS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly string[] image_extension = { ".jpg", ".jpeg", ".bmp", ".gif", ".png" }; 



        private String BrowseFolder(String path = "")
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = path;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();


            return dialog.SelectedPath.ToString();
        }

        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            tb_image_path.Text = BrowseFolder();
            if (tb_image_path.Text.Length > 0)
            {
                lv_image.Items.Clear();
                String[] files = Directory.GetFileSystemEntries(tb_image_path.Text, "*", SearchOption.TopDirectoryOnly);
                foreach (String img_file in files)
                {
                    String ext = System.IO.Path.GetExtension(img_file);
                    if (image_extension.Contains(ext)) //if the file have valid image extension
                    {
                        lv_image.Items.Add(img_file);
                    }
                }

            }
            
        }


        private String ImageToBase64(String filename)
        {
            String base64Str = "";
            String ext = "";
            try
            {
                ext = System.IO.Path.GetExtension(filename).Replace(".", "");
                byte[] imageArray = System.IO.File.ReadAllBytes(filename);

                base64Str = Convert.ToBase64String(imageArray);
            }
            catch (Exception) { }


            if (ext.Length > 0 && base64Str.Length > 0)
            {
                return String.Format("data:image/{0};base64,{1}",ext, base64Str);
            }
            else return "";
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            btn_save.IsEnabled = false;
            String filepath = "";
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();

            sfd.DefaultExt = ".css";
            sfd.Filter = "CSS Files (*.css)|*.css";

            Nullable<bool> result = sfd.ShowDialog();

            if (result == true)
            {
                filepath = sfd.FileName;
                //.[image_name] { background-image: url('[base64_string]'); }
                //String CSS_format = tb_css_format.Text.Replace("[image_name]", "{0}").Replace("[base64_string]", "{1}");
                String CSS_format = tb_css_format.Text;
                Boolean enable_image_size = false;
                if (CSS_format.Contains("[image_width]") || CSS_format.Contains("[image_height]")) enable_image_size = true;


                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs,Encoding.UTF8))
                    {

                        foreach (var item in lv_image.Items)
                        {
                            String image_name_class_name = System.IO.Path.GetFileName(item.ToString()).Replace(".", "-");
                            String base64str = ImageToBase64(item.ToString());
                            int width = 0;
                            int height = 0;

                            if (enable_image_size) {
                                using (System.Drawing.Image img = System.Drawing.Image.FromFile(item.ToString()))
                                {
                                    width = img.Width;
                                    height = img.Height;
                                }
                            }
                            
                            

                            if (base64str.Length > 0)
                            {
                                //String tmp = String.Format(CSS_format, image_name_class_name, base64str);
                                String tmp = CSS_format.Replace("[image_name]", image_name_class_name).Replace("[base64_string]", base64str);
                                if (enable_image_size) tmp = tmp.Replace("[image_width]", width.ToString() + "px").Replace("[image_height]", height.ToString() + "px");

                                sw.WriteLine(tmp);
                                
                            }
                            //MessageBox.Show(item.ToString() + "  file name:" + System.IO.Path.GetFileName(item.ToString()));
                        }
                        sw.Close();
                    }
                }

            }
            btn_save.IsEnabled = true;
            MessageBox.Show("Completed");


        }

        private void cb_enable_image_size_Checked(object sender, RoutedEventArgs e)
        {
            tb_css_format.Text = ".[image_name] { width:[image_width]; height:[image_height]; background-image: url('[base64_string]'); }";

        }

        private void cb_enable_image_size_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_css_format.Text = ".[image_name] { background-image: url('[base64_string]'); }";
        }
    }
}
