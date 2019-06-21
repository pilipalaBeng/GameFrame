#if UNITY_EDITOR_WIN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using System.Text;
using System;


namespace GameLib
{

    [CreateAssetMenu(menuName = "JsonConvert/ScriptableParse")]
    public class ScriptableParse : ScriptableObject
    {
        //!
        public int MinRowCount = 2;
        //!
        public int InvalidCommentRowIndex = 0;
        //! indicate where excel files located
        public string ExcelPath = "";
        //! indicate where to save that json files are parsed.
        public string SaveJsonPath = "";
        //!
        public string SaveCSharpPath = "";
        //!
        public string CSharpDefineName = "Excel2CSharpDefine";
        //!
        public string[] CSharpUsing = new string[]{"System"};
        //!
        public string CSharpNamespace = "cfg";

        //!
        public List<ExcelFileInfo> excelFiles = new List<ExcelFileInfo>();


        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(ExcelPath) || !Directory.Exists(ExcelPath))
            {
                var dirInfo = new DirectoryInfo(Application.dataPath);
                ExcelPath = dirInfo.Parent.Parent.Parent.FullName;
                EditorUtility.SetDirty(this);
            }

            if (string.IsNullOrWhiteSpace(SaveJsonPath))
            {
                var dirInfo = new DirectoryInfo(Application.dataPath);
                SaveJsonPath = dirInfo.Parent.Parent.Parent.FullName;
                EditorUtility.SetDirty(this);
            }

            if (string.IsNullOrWhiteSpace(SaveCSharpPath))
            {
                var dirInfo = new DirectoryInfo(Application.dataPath);
                SaveCSharpPath = dirInfo.Parent.Parent.Parent.FullName;
                EditorUtility.SetDirty(this);
            }
        }


        public virtual string Valid(DataTable sheet, SheetConfig cfg)
        {
            if (sheet.Rows.Count < MinRowCount)
            {
                return "table: " + sheet.TableName + " empty table ";
            }

            if (cfg.typeRowIndex >= sheet.Rows.Count)
            {
                return "table: " + sheet.TableName + " cfg.typeRowIndex :" + cfg.typeRowIndex + " but table total row count " + sheet.Rows.Count;
            }

            if (cfg.skipRowCount > sheet.Rows.Count)
            {
                return "table: " + sheet.TableName + " cfg.skipRowCount :" + cfg.skipRowCount + " but table total row count " + sheet.Rows.Count;
            }

            if (cfg.skipRowCount < cfg.typeRowIndex)
            {
                return "table: " + sheet.TableName + " cfg.skipRowCount :" + cfg.typeRowIndex + " but cfg.typeRowIndex: " + cfg.typeRowIndex;
            }

            var typeRow = sheet.Rows[cfg.typeRowIndex - 1];

            foreach (DataColumn column in sheet.Columns)
            {
                string name = column.ToString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return "table: " + sheet.TableName + " cloumn name null or whitespces " + column;
                }
                string typeName = typeRow[column] as string;
                if (string.IsNullOrWhiteSpace(name))
                {
                    return "table: " + sheet.TableName + " cloumn type null or whitespces " + column;
                }
                var type = getColumnType(column,cfg);
                if (type == null)
                {
                    return "table: " + sheet.TableName + " " + column + " type " + typeName + " not exsit.";
                }
            }

            return null;
        }

        public virtual object ConvertRowData(DataRow row, SheetConfig cfg)
        {
            var ret = new Dictionary<string, object>();

            foreach (DataColumn column in row.Table.Columns)
            {
                if (cfg.excludeColumns.Exists(c => c == column.ToString()))
                {
                    continue;
                }

                var name = getColumnName(column, cfg);
                object value = row[column];
                var typedValue = getValueByTypeName(getColumnType(column, cfg), value);

                if (value.GetType() == typeof(DBNull) && cfg.skipNull)
                {
                    continue;
                }

                ret[name] = typedValue;
            }

            return ret;
        }

        public virtual KeyValuePair<object, object> ConvertRowDataPair(DataRow row, SheetConfig cfg)
        {
            var ret = new Dictionary<string, object>();
            object key = null;

            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                DataColumn column = row.Table.Columns[i];

                if (cfg.excludeColumns.Exists( c => c == column.ToString()))
                {
                    continue;
                }

                var name = getColumnName(column, cfg);
                object value = row[column];
                var typedValue = getValueByTypeName(getColumnType(column, cfg), value);

                if (i == cfg.keyColumn)
                {
                    if (value.GetType() == typeof(DBNull))
                    {
                        throw new Exception("[ScriptableParse.ConvertRowDataPair] key column must not be null. row index : " + row);
                    }
                    key = typedValue;
                }

                if (value.GetType() == typeof(DBNull) && cfg.skipNull)
                {
                    continue;
                }
                ret[name] = typedValue;
            }

            return new KeyValuePair<object, object>(key, ret);
        }

        public virtual string GetCSharpDefine(DataTable sheet, ExcelFileInfo info , SheetConfig cfg)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("\t//! auto generate by excel convert.");
            sb.AppendFormat("\t//! generate from {0} sheet {1} export name {2}.", info.GetFileName() , sheet.TableName , cfg.GetExportName() );
            sb.AppendLine();
            sb.AppendLine("\t[Serializable]");
            sb.AppendFormat("\tpublic class {0} \n\t{{\n",cfg.GetExportName());

            foreach (DataColumn column in sheet.Columns)
            {
                if (cfg.excludeColumns.Exists(c => c == column.ToString()))
                {
                    continue;
                }
                
                var name = getColumnName(column, cfg);
                var typeName = getColumnType(column, cfg);
                var comment = "";
                if (cfg.commentRowIndex != InvalidCommentRowIndex && cfg.commentRowIndex < sheet.Rows.Count)
                {
                    comment = sheet.Rows[cfg.commentRowIndex - 1][column].ToString();
                }
                sb.AppendFormat("\t\tpublic {0} {1}; \t // {2}", typeName, name , comment);
                sb.AppendLine();
            }
            sb.Append("\t}");
            sb.AppendLine();

            return sb.ToString();
        }

        private object getValueByTypeName(string name, object obj)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            bool needDefaultValue = obj.GetType() == typeof(DBNull);

            switch (name)
            {
                case "uint":
                    {
                        if (needDefaultValue)
                        {
                            return getDefaultValue<uint>();
                        }
                        else
                        {
                            Debug.Assert(obj.GetType() == typeof(double));
                            double d = (double)obj;
                            return (uint)d;
                        }
                    }
                case "int":
                    {
                        if (needDefaultValue)
                        {
                            return getDefaultValue<int>();
                        }
                        else
                        {
                            Debug.Assert(obj.GetType() == typeof(double));
                            double d = (double)obj;
                            return (int)d;
                        }
                    }
                case "float":
                    {
                        if (needDefaultValue)
                        {
                            return getDefaultValue<float>();
                        }
                        else
                        {
                            Debug.Assert(obj.GetType() == typeof(double));
                            double d = (double)obj;
                            return (float)d;
                        }
                    }
                case "double":
                    {
                        if (needDefaultValue)
                        {
                            return getDefaultValue<float>();
                        }
                        else
                        {
                            Debug.Assert(obj.GetType() == typeof(double));
                            double d = (double)obj;
                            return d;
                        }
                    }
                case "string":
                    {
                        if (needDefaultValue)
                        {
                            return getDefaultValue<float>();
                        }
                        else
                        {
                            return obj.ToString();
                        }
                    }
                case "uint[]":
                    {
                        return getArrayValue<uint>(obj, needDefaultValue);
                    }
                case "int[]":
                    {
                        return getArrayValue<int>(obj, needDefaultValue);
                    }
                case "float[]":
                    {
                        return getArrayValue<float>(obj, needDefaultValue);
                    }
                case "double[]":
                    {
                        return getArrayValue<double>(obj, needDefaultValue);
                    }
                case "string[]":
                    {
                        return getArrayValue<string>(obj, needDefaultValue);
                    }
                case "uint[][]":
                    {
                        return getArray2DValue<uint>(obj, needDefaultValue);
                    }
                case "int[][]":
                    {
                        return getArray2DValue<int>(obj, needDefaultValue);
                    }
                case "float[][]":
                    {
                        return getArray2DValue<float>(obj, needDefaultValue);
                    }
                case "double[][]":
                    {
                        return getArray2DValue<double>(obj, needDefaultValue);
                    }
                case "string[][]":
                    {
                        return getArray2DValue<string>(obj, needDefaultValue);
                    }
                case "jobject":
                    {
                        if (needDefaultValue)
                        {
                            return null;
                        }
                        else
                        {
                            Debug.Assert(obj.GetType() == typeof(string));
                            return Newtonsoft.Json.Linq.JObject.Parse((string)obj);
                        }
                    }
                case "jarray":
                    {
                        if (needDefaultValue)
                        {
                            return null;
                        }
                        else
                        {
                            Debug.Assert(obj.GetType() == typeof(string));
                            return Newtonsoft.Json.Linq.JArray.Parse((string)obj);
                        }
                    }
                default:
                    {
                        var o = getObjectValue(name, obj, needDefaultValue);
                        if (o == null)
                        {
                            throw new NotSupportedException("[ScriptableParse.getTypeByName] unsupport type name " + name);
                        }
                        return o;
                    }
            }
        }
        private T getDefaultValue<T>()
        {
            return default(T);
        }

        private T[] getArrayValue<T>(object obj, bool isDefault)
        {
            if (isDefault)
            {
                return default(T[]);
            }
            else
            {
                Debug.Assert(obj.GetType() == typeof(string));
                return JsonConvert.DeserializeObject<T[]>(obj.ToString());
            }
        }

        private T[][] getArray2DValue<T>(object obj, bool isDefault)
        {
            if (isDefault)
            {
                return default(T[][]);
            }
            else
            {
                Debug.Assert(obj.GetType() == typeof(string));
                return JsonConvert.DeserializeObject<T[][]>(obj.ToString());
            }
        }

        protected virtual object getObjectValue(string typeName, object obj, bool isDefault)
        {
            if (isDefault)
            {
                return Activator.CreateInstance(Type.GetType(typeName));
            }
            else
            {
                return null;
            }
        }

        protected string getColumnName(DataColumn column, SheetConfig cfg)
        {
            string name = column.ToString();
            if (cfg.lowerCase)
            {
                name = name.ToLower();
            }
            return name;
        }

        protected string getColumnType(DataColumn column, SheetConfig cfg)
        {
            var typeRow = column.Table.Rows[cfg.typeRowIndex - 1];
            return typeRow[column] as string;
        }

    }
}
#endif