#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using System.Text;
using System;
using Excel;

namespace GameLib
{
    [Serializable]
    public sealed class ExcelFileInfo
    {
        //!
        public string path;
        //!
        [NonSerialized]
        public string error;
        //!
        [NonSerialized]
        public bool selected = false;
        //!
        [NonSerialized]
        public bool flodout = false;
        //!
        public List<SheetConfig> sheetCfgs = new List<SheetConfig>();
        //!
        public string GetFileName()
        {
            return Path.GetFileName(path);
        }
    }

    public enum ConvertMode
    {
        ConvertMode_JObject,
        ConvertMode_JArray,
    }


    [Serializable]
    public sealed class SheetConfig
    {
        //!
        public string sheetName;
        //!
        [NonSerialized]
        public string error;
        //!
        [NonSerialized]
        public bool selected = true;
        //!
        public string exportName;
        //!
        public ConvertMode convertMode = ConvertMode.ConvertMode_JObject;
        //!
        public int skipRowCount = 3; // default 3
        //!
        public bool lowerCase = false;
        //!
        public bool skipNull = true;
        //!
        public int typeRowIndex = 1;
        //!
        public int keyColumn = 0;
        //!
        public int commentRowIndex = 2;
        //!
        public List<string> excludeColumns = new List<string>();
        //!
        [NonSerialized]
        public bool flodout = false;
        //!
        [NonSerialized]
        public bool showColumnsFlodout = false;
        //!
        [NonSerialized]
        public List<string> columnNames = new List<string>();

        public string GetExportName()
        {
            return string.IsNullOrWhiteSpace(exportName) ? sheetName : exportName;
        }
    }

    //TODO list
    // 1.exclude cloumn
    // 2.multi_sheet mode
    //{
    //  muti config
    //  multi error
    //}
    // 3.chinese
    // 4.exe
    public class ExcelConvert : EditorWindow
    {
        #region default setting
        //!
        private string mDefaultCoding = "utf-8";
        //!
        private UnityEngine.Object mParseScript;
        #endregion

        //!
        private Texture2D mExcelIcon = null;
        //!
        private Texture2D mExcelErrorIcon = null;
        //!
        private string mShowContent = "";


        private ScriptableParse scriptableParse
        {
            get { return (ScriptableParse)mParseScript; }
        }

        [MenuItem("JsonConvert/Open %j")]
        static void OpenWindow()
        {
            var w = GetWindow<ExcelConvert>(false);
            w.Init();
        }

        void Init()
        {
            var t = new GUIContent("Excel Convert", (Texture2D)EditorGUIUtility.Load("JsonConvert/Icons/icon.png"));
            titleContent = t;
            mExcelIcon = (Texture2D)EditorGUIUtility.Load("JsonConvert/Icons/excel_icon.png");
            mExcelErrorIcon = (Texture2D)EditorGUIUtility.Load("JsonConvert/Icons/excel_error_icon.png");

            mParseScript = (ScriptableParse)EditorGUIUtility.Load("JsonConvert/DefaultParse.asset");

            var allCoding = Encoding.GetEncodings();
            mCoding = new string[allCoding.Length];
            for (int i = 0; i < allCoding.Length; i++)
            {
                mCoding[i] = allCoding[i].GetEncoding().HeaderName;
                if (mCoding[i].Equals(mDefaultCoding))
                {
                    mCurrentCodingIndex = i;
                }
            }

            scriptableParse.excelFiles.ForEach(f => checkAndInitFile(f));
        }

        void OnGUI()
        {
            onOptGUI();
            onExcelFilesGUI();
            onContentGUI();
            onJsonCfgGUI();
        }

        #region GUI Opt
        //!
        string[] mCoding = null;
        //!
        int mCurrentCodingIndex = 0;
        //!
        string log;
        void onOptGUI()
        {
            GUILayout.BeginArea(new Rect(10, position.height * 0.5f, position.width * 0.5f - 10, position.height * 0.5f - 20), GUI.skin.window);
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("coding: ");
                    mCurrentCodingIndex = EditorGUILayout.Popup(mCurrentCodingIndex, mCoding);
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                mParseScript = EditorGUILayout.ObjectField(mParseScript, typeof(ScriptableParse), false);

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("选择json导出目录", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        onJsonSaveDirBtn();
                    }
                    GUILayout.Label("当前目录:\n  " + scriptableParse.SaveJsonPath);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("选择c#导出目录", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        onCSharpSaveDirBtn();
                    }
                    GUILayout.Label("当前目录:\n  " + scriptableParse.SaveCSharpPath);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("添加 Excel 文件"))
                {
                    onAddFileBtn();
                }

                if (GUILayout.Button("添加目录下的所有excel文件"))
                {
                    onAddDirBtn();
                }

                if (GUILayout.Button("删除选中文件"))
                {
                    removeSelectedFiles();
                }

                if (GUILayout.Button("删除所有文件"))
                {
                    clearExcelFiles();
                }

                if (GUILayout.Button("导出json"))
                {
                    log = "";
                    onExport();
                }
                if (GUILayout.Button("导出c#结构"))
                {
                    log = "";
                    onExportCSharp();
                }
                if (!string.IsNullOrWhiteSpace(log))
                {
                    GUILayout.TextArea(log);
                }
            }
            GUILayout.EndArea();
        }
        #endregion

        #region GUI Excel files
        //! grid view
        Vector2 mExcelScollPos = Vector2.zero;
        int mExcelUIColumn = 4;
        float mLeftPadding = 10.0f;
        float mTopPadding = 10.0f;
        bool mSelectAll = false;
        void onExcelFilesGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, position.width * 0.5f - 10, position.height * 0.5f - 20), GUI.skin.window);
            GUILayout.BeginVertical(GUI.skin.box);
            mExcelUIColumn = EditorGUILayout.IntSlider("column count:", mExcelUIColumn, 1, 10);

            // select all diselect all
            var orignal = mSelectAll;
            mSelectAll = EditorGUILayout.Toggle("select all", mSelectAll);
            if (orignal != mSelectAll)
            {
                onSelectAll(mSelectAll);
            }

            EditorGUILayout.Separator();
            if (scriptableParse.excelFiles.Count > 0)
            {
                mExcelScollPos = GUILayout.BeginScrollView(mExcelScollPos, GUI.skin.box);
                GUILayout.Space(mTopPadding);
                for (int i = 0; i < scriptableParse.excelFiles.Count; i++)
                {
                    var file = scriptableParse.excelFiles[i];

                    if (i % mExcelUIColumn == 0)
                    {
                        if (i != 0)
                        {
                            // end previous 
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(mLeftPadding);
                    }

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        var icon = string.IsNullOrEmpty(file.error) ? mExcelIcon : mExcelErrorIcon;
                        file.selected = GUILayout.Toggle(file.selected, icon, GUILayout.Width(100f), GUILayout.Height(100f));
                        GUILayout.Label(Path.GetFileName(file.path), GUILayout.Width(100f), GUILayout.Height(20f));
                    }
                    GUILayout.EndVertical();

                    if (i == scriptableParse.excelFiles.Count - 1)
                    {
                        // end last begin horizontal
                        GUILayout.EndHorizontal();
                    }

                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        #endregion

        #region GUI json detail
        Vector2 mScrollPos = Vector2.zero;
        List<ExcelFileInfo> mNeedRemove = new List<ExcelFileInfo>();
        void onJsonCfgGUI()
        {
            if (mPeeking)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(position.width * 0.5f, 10, position.width * 0.5f - 10, position.height - 20), GUI.skin.window);
            GUILayout.BeginVertical();
            mScrollPos = GUILayout.BeginScrollView(mScrollPos);
            {
                foreach (ExcelFileInfo f in scriptableParse.excelFiles)
                {
                    if (!f.selected)
                    {
                        continue;
                    }

                    f.flodout = EditorGUILayout.Foldout(f.flodout, f.GetFileName() + ": 配置详情.");
                    if (!f.flodout)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(25);
                        GUILayout.BeginVertical();
                        {
                            if (string.IsNullOrEmpty(f.error))
                            {
                                f.sheetCfgs.ForEach(sheet =>
                                {
                                    sheetGUI(f, sheet);
                                });
                            }
                            else
                            {
                                EditorGUILayout.LabelField("error: ",f.error);
                            }
                            
                            if (GUILayout.Button("Remove file"))
                            {
                                mNeedRemove.Add(f);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
                if (mNeedRemove.Count > 0)
                {
                    mNeedRemove.ForEach(f => removeExcelFile(f));
                    mNeedRemove.Clear();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void sheetGUI(ExcelFileInfo info, SheetConfig cfg)
        {
            cfg.flodout = EditorGUILayout.Foldout(cfg.flodout, cfg.sheetName + ": 详情.");
            if (!cfg.flodout)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(25);
                GUILayout.BeginVertical();
                {
                    cfg.selected =      EditorGUILayout.Toggle("是否导出: ", cfg.selected);
                    cfg.exportName =    EditorGUILayout.TextField("导出名: ", cfg.exportName);
                    cfg.convertMode =   (ConvertMode)EditorGUILayout.EnumPopup("导出模式: ", cfg.convertMode);
                    cfg.skipRowCount =  EditorGUILayout.IntSlider("跳过行数: ", cfg.skipRowCount, 2, 10);
                    cfg.lowerCase =     EditorGUILayout.Toggle("是否小写: ", cfg.lowerCase);
                    cfg.skipNull =      EditorGUILayout.Toggle("是否跳过空单元: ", cfg.skipNull);
                    cfg.typeRowIndex =  EditorGUILayout.IntSlider("类型索引: ", cfg.typeRowIndex, 1, 10);
                    cfg.commentRowIndex =   EditorGUILayout.IntSlider(string.Format("注释索引({0}:ignore commint): " , scriptableParse.InvalidCommentRowIndex), cfg.commentRowIndex, 0, 10);
                    if (cfg.convertMode == ConvertMode.ConvertMode_JObject)
                    {
                        cfg.keyColumn = EditorGUILayout.IntSlider("主键列: ", cfg.keyColumn, 0, 10);
                    }
                    excludeColumnsGUI(cfg);
                    if (GUILayout.Button("保存配置"))
                    {
                        onSaveConfig();
                    }
                    if (GUILayout.Button("json 预览"))
                    {
                        onTakePeek(info, cfg);
                        mPeeking = true;
                    }

                    if (GUILayout.Button("c# 预览"))
                    {
                        onTakePeekCSharp(info, cfg);
                        mPeeking = true;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        void excludeColumnsGUI(SheetConfig cfg)
        {
            if (!string.IsNullOrEmpty(cfg.error))
            {
                EditorGUILayout.LabelField("error: ", cfg.error);
                return;
            }

            cfg.showColumnsFlodout = EditorGUILayout.Foldout(cfg.showColumnsFlodout, "导出列选择.");
            if (!cfg.showColumnsFlodout)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(25);
                GUILayout.BeginVertical();
                {
                    cfg.columnNames.ForEach(c =>
                    {
                        var exportColumn = cfg.excludeColumns.Find(ec => ec == c) == null;
                        var export = GUILayout.Toggle(exportColumn, c);
                        if (export != exportColumn)
                        {
                            if (export)
                            {
                                cfg.excludeColumns.Remove(c);
                            }
                            else
                            {// exclude this column
                                cfg.excludeColumns.Add(c);
                            }
                            EditorUtility.SetDirty(scriptableParse);
                        }
                    });
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }



        #endregion

        #region GUI json content
        Vector2 mContentScollPos = Vector2.zero;
        bool mPeeking = false;
        void onContentGUI()
        {
            if (!mPeeking)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(position.width * 0.5f, 10, position.width * 0.5f - 10, position.height - 20), GUI.skin.window);
            GUILayout.BeginVertical();
            {
                if (GUILayout.Button(" 返回 "))
                {
                    mPeeking = false;
                }

                if (!string.IsNullOrWhiteSpace(mShowContent))
                {
                    mContentScollPos = GUILayout.BeginScrollView(mContentScollPos, GUI.skin.box);
                    {
                        GUILayout.TextArea(mShowContent);
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        void addExcelFile(ExcelFileInfo info)
        {
            if (scriptableParse.excelFiles.Find(f => f.GetFileName() == info.GetFileName()) != null)
            {
                throw new Exception("[ExcelConvert.addExcelFile] file " + info.path + "already selected.");
            }

            scriptableParse.excelFiles.Add(info);
            EditorUtility.SetDirty(scriptableParse);
            Repaint();
        }

        void removeExcelFile(ExcelFileInfo info)
        {
            scriptableParse.excelFiles.Remove(info);
            EditorUtility.SetDirty(scriptableParse);
            Repaint();
        }

        void clearExcelFiles()
        {
            scriptableParse.excelFiles.Clear();
            EditorUtility.SetDirty(scriptableParse);
            Repaint();
        }
        #endregion

        #region events & handle
        void onAddDirBtn()
        {
            var orignalPath = scriptableParse.ExcelPath;
            scriptableParse.ExcelPath = EditorUtility.OpenFolderPanel("select excels folder", orignalPath, "");

            if (string.IsNullOrWhiteSpace(scriptableParse.ExcelPath))
            {
                scriptableParse.ExcelPath = orignalPath;
                return;
            }

            EditorUtility.SetDirty(scriptableParse);

            onOpenDir();
        }

        void onAddFileBtn()
        {
            var orignalPath = scriptableParse.ExcelPath;
            var fileName = EditorUtility.OpenFilePanel("select excels folder", orignalPath, "xlsx");

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            EditorUtility.SetDirty(scriptableParse);

            checkAndAddFile(fileName);
        }

        void onOpenDir()
        {
            var files = Directory.GetFiles(scriptableParse.ExcelPath, "*.xlsx");

            if (files == null || files.Length == 0)
            {
                ShowNotification(new GUIContent("xlsx files not found."));
                return;
            }

            foreach (string f in files)
            {
                checkAndAddFile(f);
            }
        }

        void onJsonSaveDirBtn()
        {
            var orignalPath = scriptableParse.SaveJsonPath;
            scriptableParse.SaveJsonPath = EditorUtility.OpenFolderPanel("select save folder", orignalPath, "");

            if (string.IsNullOrWhiteSpace(scriptableParse.SaveJsonPath))
            {
                scriptableParse.SaveJsonPath = orignalPath;
                return;
            }

            EditorUtility.SetDirty(scriptableParse);
        }

        void onCSharpSaveDirBtn()
        {
            var orignalPath = scriptableParse.SaveCSharpPath;
            scriptableParse.SaveCSharpPath = EditorUtility.OpenFolderPanel("select save folder", orignalPath, "");

            if (string.IsNullOrWhiteSpace(scriptableParse.SaveCSharpPath))
            {
                scriptableParse.SaveCSharpPath = orignalPath;
                return;
            }

            EditorUtility.SetDirty(scriptableParse);
        }

        void onSelectAll(bool all)
        {
            scriptableParse.excelFiles.ForEach(f => f.selected = all);
        }

        void onSaveConfig()
        {
            EditorUtility.SetDirty(scriptableParse);
            Repaint();
        }

        void onExport()
        {
            safeForeachSelectedSheets((f, sheet, cfg) =>
            {
                var path = Path.Combine(scriptableParse.SaveJsonPath, cfg.GetExportName() + ".json");
                using (FileStream jsonFile = File.Open(path, FileMode.Create, FileAccess.Write))
                {
                    using (TextWriter tw = new StreamWriter(jsonFile, getCurrentCoding()))
                    {
                        string strJson = convertSheet(sheet, cfg);
                        tw.Write(strJson);
                        tw.Flush();
                        tw.Close();
                        log += string.Format("{0}->{1}->{2}.json export SUCCESS.\n", f.GetFileName(), cfg.sheetName, cfg.GetExportName());
                    }
                }
            });
        }

        Encoding getCurrentCoding()
        {
            Encoding cd = new UTF8Encoding(false);
            if (mCoding[mCurrentCodingIndex] != "utf8-nobom")
            {
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    Encoding e = ei.GetEncoding();
                    if (e.HeaderName == mCoding[mCurrentCodingIndex])
                    {
                        cd = e;
                        break;
                    }
                }
            }
            return cd;
        }

        delegate void SheetAction(ExcelFileInfo info , DataTable sheet , SheetConfig cfg);

        void safeForeachSelectedSheets(SheetAction func)
        {
            foreach (ExcelFileInfo f in scriptableParse.excelFiles)
            {
                try
                {
                    if (!f.selected)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(f.error))
                    {
                        using (FileStream excelFile = File.Open(f.path, FileMode.Open, FileAccess.Read))
                        {
                            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile);
                            excelReader.IsFirstRowAsColumnNames = true;
                            DataSet book = excelReader.AsDataSet();

                            foreach (DataTable sheet in book.Tables)
                            {
                                var cfg = f.sheetCfgs.Find(fcfg => fcfg.sheetName == sheet.TableName);
                                if (cfg == null)
                                {
                                    throw new Exception(f.path + " sheet " + sheet.TableName + " config not found.");
                                }

                                if (!cfg.selected)
                                {
                                    continue;
                                }

                                if (string.IsNullOrEmpty(cfg.error))
                                {
                                    func( f , sheet ,cfg );
                                }
                                else
                                {
                                    log += string.Format("{0}->{1}->{2} export FAILED. caused by {3}.\n", f.GetFileName(), cfg.sheetName, cfg.GetExportName(), cfg.error);
                                }
                            }

                        }
                    }
                    else
                    {
                        log += string.Format("File {0} export FAILED. caused by {1}.\n", f.GetFileName(), f.error);
                    }
                }
                catch (Exception e)
                {
                    ShowNotification(new GUIContent(Path.GetFileNameWithoutExtension(f.path) + e.Message));
                }
            }
        }

        void onExportCSharp()
        {

            StringBuilder sb = new StringBuilder();

            if (scriptableParse.CSharpUsing.Length > 0)
            {
                Array.ForEach(scriptableParse.CSharpUsing, s => sb.AppendFormat("using {0};\n", s));
            }
            sb.Append("\n");
            if (!string.IsNullOrWhiteSpace(scriptableParse.CSharpNamespace))
            {
                sb.AppendFormat("namespace {0}\n{{\n", scriptableParse.CSharpNamespace);
            }
            sb.Append("\n");
            safeForeachSelectedSheets(( f , sheet , cfg )=> 
            {
                sb.Append(scriptableParse.GetCSharpDefine(sheet, f, cfg));
                sb.AppendLine();
                log += string.Format("{0}->{1}->{2} export SUCCESS.\n", f.GetFileName(), cfg.sheetName, cfg.GetExportName());
            });

            if (!string.IsNullOrWhiteSpace(scriptableParse.CSharpNamespace))
            {
                sb.Append("}");
            }

            var path = Path.Combine(scriptableParse.SaveCSharpPath, scriptableParse.CSharpDefineName + ".cs");
            using (FileStream jsonFile = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter tw = new StreamWriter(jsonFile, getCurrentCoding()))
                {
                    tw.Write(sb.ToString());
                    tw.Flush();
                    tw.Close();

                }
            }
        }

        void onTakePeek(ExcelFileInfo info, SheetConfig cfg)
        {
            mShowContent = convertToJson(info, cfg);
        }

        void onTakePeekCSharp(ExcelFileInfo info, SheetConfig cfg)
        {
            mShowContent = convertCSharpDefine( info , cfg );
        }

        void removeSelectedFiles()
        {
            if (scriptableParse.excelFiles.RemoveAll( ef => ef.selected ) > 0)
            {
                EditorUtility.SetDirty(scriptableParse);
            }
            
        }
        #endregion

        #region parse json
        //!
        void checkAndAddFile(string file)
        {
            var info = scriptableParse.excelFiles.Find(ef => ef.GetFileName() == Path.GetFileName(file));
            bool needAdd = false;
            if (info == null)
            {
                info = new ExcelFileInfo
                {
                    path = file
                };
                needAdd = true;
            }

            checkAndInitFile(info);

            if (!string.IsNullOrEmpty(info.error))
            {
                ShowNotification(new GUIContent(Path.GetFileNameWithoutExtension(file) + info.error));
                return;
            }

            if (needAdd)
            {
                addExcelFile(info);
            }

        }


        //!
        void checkAndInitFile(ExcelFileInfo info)
        {
            try
            {
                using (FileStream excelFile = File.Open(info.path, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile);
                    excelReader.IsFirstRowAsColumnNames = true;
                    DataSet book = excelReader.AsDataSet();
                    // test if empty
                    if (book.Tables.Count < 1)
                    {
                        info.error = "empty excel file.";
                        return;
                    }


                    foreach (DataTable sheet in book.Tables)
                    {
                        var cfg = info.sheetCfgs.Find(scfg => scfg.sheetName == sheet.TableName);
                        if (cfg == null)
                        {
                            cfg = new SheetConfig();
                            cfg.sheetName = sheet.TableName;
                            info.sheetCfgs.Add(cfg);
                        }
                        cfg.columnNames.Clear();
                        foreach (DataColumn c in sheet.Columns)
                        {
                            cfg.columnNames.Add(c.ToString());
                        }
                        var err = scriptableParse.Valid(sheet, cfg);
                        if (!string.IsNullOrEmpty(err))
                        {
                            cfg.error = err;
                        }
                    }
                    if (info.sheetCfgs.RemoveAll(scfg =>
                    {
                        return !book.Tables.Contains(scfg.sheetName);
                    }) > 0)
                    {
                        EditorUtility.SetDirty(scriptableParse);
                    }
                }
            }
            catch (Exception e)
            {
                info.error = e.Message;
            }
        }


        //!
        string convertToJson(ExcelFileInfo info, SheetConfig cfg)
        {

            if (!string.IsNullOrEmpty(info.error))
            {
                return info.error;
            }
            if (!string.IsNullOrEmpty(cfg.error))
            {
                return info.error;
            }
            try
            {
                using (FileStream excelFile = File.Open(info.path, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile);
                    excelReader.IsFirstRowAsColumnNames = true;
                    DataSet book = excelReader.AsDataSet();

                    var sheet = book.Tables[cfg.sheetName];
                    return convertSheet(sheet, cfg);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        string convertCSharpDefine(ExcelFileInfo info, SheetConfig cfg)
        {
            if (!string.IsNullOrEmpty(info.error))
            {
                return info.error;
            }
            if (!string.IsNullOrEmpty(cfg.error))
            {
                return info.error;
            }
            try
            {
                using (FileStream excelFile = File.Open(info.path, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile);
                    excelReader.IsFirstRowAsColumnNames = true;
                    DataSet book = excelReader.AsDataSet();

                    var sheet = book.Tables[cfg.sheetName];
                    return scriptableParse.GetCSharpDefine(sheet,info , cfg);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        string convertSheet(DataTable sheet, SheetConfig cfg)
        {
            if (!string.IsNullOrWhiteSpace(cfg.error))
            {
                return cfg.error;
            }
            if (cfg.convertMode == ConvertMode.ConvertMode_JObject)
            {
                return convertDictionary(sheet, cfg);
            }
            else
            {
                return convertArray(sheet, cfg);
            }
        }

        string convertArray(DataTable sheet, SheetConfig cfg)
        {
            List<object> values = new List<object>();

            int firstDataRow = cfg.skipRowCount - 1;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];
                values.Add(scriptableParse.ConvertRowData(row, cfg));
            }

            //-- convert to json string
            return JsonConvert.SerializeObject(values, Formatting.Indented);
        }

        string convertDictionary(DataTable sheet, SheetConfig cfg)
        {
            Dictionary<object, object> values = new Dictionary<object, object>();

            int firstDataRow = cfg.skipRowCount - 1;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];
                var kvPair = scriptableParse.ConvertRowDataPair(row, cfg);
                values.Add(kvPair.Key, kvPair.Value);
            }

            //-- convert to json string
            return JsonConvert.SerializeObject(values, Formatting.Indented);
        }
        #endregion
    }
}

#endif