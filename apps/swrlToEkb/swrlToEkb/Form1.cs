﻿using swrlToEkb.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace swrlToEkb
{
    public partial class Form1 : Form
    {
        string filename; //Путь к файлу
        string pathToDid; //Путь к директории
        string ekbText = "";
        bool status = false;

        //Листы объектов классов
        List<SWRL> swrl;
        List<Association> associations;
        public Form1()
        {
            InitializeComponent();
            openFileDialog1.Filter = "Файл owl (*.owl)|*.owl|All files(*.*)|*.*"; //Фильтр файлов
            convertBtn.Enabled = false;

            swrl = new List<SWRL>();
            associations = new List<Association>();
        }

        private void fileBtn_Click(object sender, EventArgs e) //Слушатель кнопки выбора файла
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                fileLbl.Text = "Ошибка загрузки файла!";
                fileLbl.ForeColor = Color.Red;
                filename = null;
            }
            else
            {
                // получаем выбранный файл
                filename = openFileDialog1.FileName;
                fileLbl.Text = "Файл успешно загружен";
                fileLbl.ForeColor = Color.Green;

            }
            checkToReady();
        }

        private void saveBtn_Click(object sender, EventArgs e) //Слушатель кнопки выбора директории
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            FBD.ShowNewFolderButton = false;
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                saveLbl.Text = "Директория для \r\nсохранения успешно выбрана!";
                saveLbl.ForeColor = Color.Green;
                pathToDid = FBD.SelectedPath;
            }
            else
            {
                saveLbl.Text = "Ошибка выбора \r\nдиректории для сохранения";
                pathToDid = null;
                saveLbl.ForeColor = Color.Red;
            }
            checkToReady();
        }

        private void checkToReady() //Метод проверки выбора директории и файла
        {
            if (filename != null && pathToDid != null)
            {
                convertBtn.Enabled = true;
            }
            else
            {
                convertBtn.Enabled = false;
            }
        }

        private void parseTheFile() //Метод парсинга файла
        {
            try
            {
                string attr = "";
                string className = "";

                string start = "";
                string end = "";
                string typeText = "";
                string assocName = "";

                List<(string, string)> Attribute;

                filename = openFileDialog1.FileName;

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(filename);
                XmlElement xRoot = xDoc.DocumentElement;
                if (xRoot != null) //Проверка корня файла на пустоту
                {
                    var child = xRoot.ChildNodes; //Начало парсинга
                    if (child.Item(1) == null)
                    {
                        status = false; 
                        return;
                    }
                    var classes = child.Item(1).ChildNodes;
                    
                    for (int i = 0; i < classes.Count; i ++) //Парсинг классов
                    {
                        Attribute = new List<(string, string)>();
                        var cl = classes[i];
                        className = cl.Attributes["rdf:ID"].Value;
                        var classChildren = cl.ChildNodes;
                        for(int j = 0; j < classChildren.Count; j++) //Парсинг переменных
                        {
                            var attrib = classChildren[j];
                            typeText = attrib.Attributes["xml:type"].Value;
                            attr = attrib.InnerText;
                            Attribute.Add((typeText, attr));
                        }
                        swrl.Add(new SWRL(className, Attribute));
                    }

                    var imp = child.Item(3).ChildNodes;

                    var assocChild = imp.Item(0).ChildNodes;
                    for(int i = 0; i < assocChild.Count; i++) //Парсинг связей классов
                    {
                        var assoc = assocChild[i];
                        assocName = assoc.Attributes["swrl:property"].Value;
                        var nodes = assoc.ChildNodes;
                        start = nodes.Item(0).InnerText;
                        end = nodes.Item(1).InnerText;
                        associations.Add(new Association(assocName, start, end));
                    }
                    Console.WriteLine("efefefef");

                }
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                return;
            }
        }

        private void saveToEkb() //Метод сохранения файла
        {
            string path = $"{pathToDid}/file.ekb";
            System.IO.File.WriteAllText(path, ekbText);
            MessageBox.Show("Файл сохранен");
        }

        private void convertToEKB() //Метод преобразования файла в формат EKB
        {
            //Заполнение шапки структуры
            Random random = new Random();
            string id = $"{random.Next(1000000000)}{random.Next(100)}";
            string header = $"\r\n<Structure>"
                + $"\r\n<KnowledgeBase>"
                + $"\r\n<ID>{id}</ID>"
                + $"\r\n<Name>База знаний 1</Name>"
                + $"\r\n<ShortName>Baza-znaniy-1</ShortName>"
                + $"\r\n<Kind>0</Kind>"
                + $"\r\n<Description></Description>"
                + $"\r\n<Vars/>"
                + $"\r\n<Templates>";

            string templates = "";
            string slots = "";
            string templateEnd = "";
            string templatesEnd = "";
            string grules = "";

            int l = 0;

            ekbText = header;
            int count = 1;
            while (l < swrl.Count) //Заполнение конструкций классов
            {
                string idTempStr = "";
                if (count < 10)
                {
                    idTempStr = "";
                    idTempStr = $"00{count}";
                }
                if (count >= 10)
                {
                    idTempStr = "";
                    idTempStr = $"0{count}";
                }
                templates = $"\r\n<Template>" +
                $"\r\n<ID>T{idTempStr}</ID>" +
                $"\r\n<Name>{swrl[l].ClassName}</Name>" +
                $"\r\n<ShortName>{swrl[l].ClassName}</ShortName>" +
                $"\r\n<Description>Описание шаблона T{idTempStr}</Description>" +
                $"\r\n<PackageName></PackageName>" +
                $"\r\n<RootPackageName></RootPackageName>" +
                $"\r\n<DrawParams>xT{idTempStr}=15" +
                $"\r\nyT{idTempStr}=15" +
                $"\r\nw=265" +
                $"\r\nh=65" +
                $"\r\n</DrawParams>" +
                $"\r\n<Slots>";
                int p = 0;
                ekbText += templates;
                while (p < swrl[l].Attribute.Count) //Заполнение переменных класса
                {
                    slots = $"\r\n<Slot>" +
                    $"\r\n<Name>{swrl[l].Attribute[p].Item2}</Name>" +
                    $"\r\n<ShortName>{swrl[l].Attribute[p].Item2}</ShortName>" +
                    $"\r\n<Description>{swrl[l].Attribute[p].Item2}</Description>" +
                    $"\r\n<Value></Value>" +
                    $"\r\n<DataType>{swrl[l].Attribute[p].Item1}</DataType>" +
                    $"\r\n<Constraint></Constraint>" +
                    $"\r\n</Slot>";
                    ekbText += slots;
                    p++;
                }

                templateEnd = $"\r\n</Slots>" +
                    "\r\n</Template>";
                ekbText += templateEnd;
                l++;
                count++;
            }

            templatesEnd = templatesEnd = $"\r\n</Templates>";
            ekbText += templatesEnd;

            string facts = "\r\n<Facts/>" +
            "\r\n<GRules>";
            ekbText += facts;

            int q = 0;
            count = 1;
            while (q < associations.Count) //Заполнение связей
            {
                string idTempStr = "";
                if (count < 10)
                {
                    idTempStr = "";
                    idTempStr = $"00{count}";
                }
                if (count >= 10)
                {
                    idTempStr = "";
                    idTempStr = $"0{count}";
                }
                grules =
                    $"\r\n<GRule>" +
                    $"\r\n<ID>G{idTempStr}</ID>" +
                    $"\r\n<Name>{associations[q].AssotionName}</Name>" +
                    $"\r\n<ShortName>{associations[q].AssotionName}</ShortName>" +
                    $"\r\n<Description>{associations[q].AssotionName}</Description>" +
                    $"\r\n<PackageName></PackageName>" +
                    $"\r\n<RootPackageName></RootPackageName>" +
                    $"\r\n<DrawParams>xG{idTempStr}=26" +
                    $"\r\nyG{idTempStr}=105" +
                    $"\r\nw=170" +
                    $"\r\nh=34" +
                    $"\r\n</DrawParams>" +
                    $"\r\n<Conditions>" +
                    $"\r\n<C0>{associations[q].SourceName}</C0>" +
                    $"\r\n</Conditions>" +
                    $"\r\n<Actions>" +
                    $"\r\n<A0>{associations[q].TargetName}</A0>" +
                    $"\r\n</Actions>" +
                    $"\r\n</GRule>";
                ekbText += grules;
                q++;
                count++;
            }
            //Заполнение конца файла
            string ekbEnd = $"\r\n</GRules>" +
                $"\r\n<Rules/>" +
                $"\r\n<Functions/>" +
                $"\r\n<Tasks/>" +
                $"\r\n<FScales/>" +
                $"\r\n<TempPackageList/>" +
                $"\r\n<FactPackageList/>" +
                $"\r\n<RulePackageList/>" +
                $"\r\n<GRulePackageList/>" +
                $"\r\n</KnowledgeBase>" +
                $"\r\n</Structure>";

            ekbText += ekbEnd;
        }

        private void convertBtn_Click(object sender, EventArgs e) //Слушатель кнопки конвертации
        {
            associations.Clear();
            swrl.Clear();

            parseTheFile();
            convertToEKB();
            if (status)
            {
                saveToEkb();
            }
            else
            {
                MessageBox.Show("Проверьте файл для конвертации.", "Произошла ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
