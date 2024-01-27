﻿using System.Text;

namespace CSGenerator {
    internal class ClassStructure {
        internal string ClassName = "";
        private List<FieldStructure> fields = new();
        private List<MethodStructure> methods = new();
        internal List<ClassStructure> subClasses = new();

        internal IReadOnlyList<FieldStructure> Fields {
            get {
                List<FieldStructure> fList = [
                    .. fields.Where(p => !p.isStatic && p.isPrivate && p.name.Contains('_')),
                    .. fields.Where(p => !p.isStatic && p.isPrivate && !p.name.Contains('_')),
                    .. fields.Where(p => !p.isStatic && !p.isPrivate),
                ];

                return fList;
            }
        }

        internal IReadOnlyList<MethodStructure> Methods {
            get {
                List<MethodStructure> mList =
                [
                    .. methods.Where(p => p.isConstructor),
                    .. methods.Where(p => !p.isConstructor && !p.isStatic && p.isPrivate),
                    .. methods.Where(p => !p.isConstructor && !p.isStatic && !p.isPrivate),
                    .. methods.Where(p => !p.isConstructor && p.isStatic),
                ];

                return mList;
            }
        }

        internal ClassStructure() { }

        internal static ClassStructure BuildStructure(string className, Dictionary<string, List<Declaration>> allDecs) {
            ClassStructure c = new();

            List<Declaration> decs = allDecs[className];

            c.ClassName = className.Split('.').Last();
            foreach (var f in decs.Where(p => !p.isFunction)) {
                c.fields.Add(new FieldStructure(f));
            }

            foreach (var m in decs.Where(p => p.isFunction)) {
                if (m.isConstructor) m.name = c.ClassName;
                c.methods.Add(new MethodStructure(m));
            }

            c.subClasses = new List<ClassStructure>();
            foreach (var subDecs in allDecs.Where(p => p.Key.StartsWith(className + "."))) {
                // only create subclasses for declarations one level down from current class
                string def = subDecs.Key.Replace(className + ".", "");
                if (def.Contains(".")) continue;

                c.subClasses.Add(BuildStructure(subDecs.Key, allDecs));
            }

            return c;
        }

        internal class FieldStructure {
            internal string name;
            internal string type;
            internal bool isStatic;
            internal bool isPrivate;

            internal FieldStructure(Declaration dec) {
                name = dec.name;
                type = dec.type;
                isStatic = dec.isStatic;
                isPrivate = dec.isPrivate;
            }

            internal string Write() {
                StringBuilder sb = new();

                if (isPrivate) sb.Append("private ");
                else sb.Append("public ");

                if (isStatic) sb.Append("static ");

                sb.Append(type + " ");
                sb.AppendLine(name + ";");

                return sb.ToString();
            }
        }

        internal class MethodStructure {
            internal string name;
            internal bool isStatic;
            internal bool isPrivate;
            internal bool isConstructor;
            internal List<FieldStructure> functionParams = new();
            internal string functionReturnType;

            internal MethodStructure(Declaration dec) {
                name = dec.name;
                isStatic = dec.isStatic;
                isPrivate = dec.isPrivate;
                isConstructor = dec.isConstructor;
                functionReturnType =
                    String.IsNullOrEmpty(dec.functionReturnType) || dec.functionReturnType.ToLower() == "null"
                    ? "void"
                    : dec.functionReturnType;

                if (dec.functionParams != null) {
                    foreach (var p in dec.functionParams) {
                        FieldStructure s = new(p);
                        functionParams.Add(s);
                    }
                }
            }

            internal string Write(IReadOnlyList<FieldStructure> classFields) {
                StringBuilder sb = new();

                if (isPrivate) sb.Append("private ");
                else sb.Append("public ");

                if (!isConstructor) {
                    if (isStatic) sb.Append("static ");
                    sb.Append(functionReturnType + " ");
                }

                sb.Append(name + "(");
                if (functionParams != null) {
                    sb.Append(String.Join(',', functionParams.Select(x => x.type + " " + x.name)));
                }
                sb.AppendLine(")");
                sb.AppendLine("{");
                if (!isConstructor) {
                    sb.Append("throw new NotImplementedException();");
                } else if (functionParams != null) {
                    foreach (var fParam in functionParams) {
                        var matching = classFields
                            .FirstOrDefault(x => x.type.Equals(fParam.type) &&
                                (x.name.ToLower().Equals(fParam.name.ToLower().Replace("_", "")) ||
                                x.name.ToLower().Equals(fParam.name.ToLower())));

                        if (matching != null) {
                            sb.AppendLine(String.Format("this.{0} = {1};", matching.name, fParam.name));
                        }
                    }
                }

                sb.AppendLine("}\n");

                return sb.ToString();
            }
        }
    }
}
