﻿using static CSGenerator.Utils;

namespace CSGenerator {
    internal class Parser {
        internal string rootClassName = "";
        internal string rootNamespace = "";
        internal string templateName = "";
        internal ClassStructure? rootClass;

        internal void Parse(string filePath) {
            var lines = ReadFile(filePath)
                .Where(p => !string.IsNullOrEmpty(p) && !p.Trim().StartsWith("//"))
                .ToArray();

            if (lines.Length <= 2) {
                throw new Exception("This file is empty.");
            }

            if (lines[0].StartsWith("@")) {
                templateName = lines[0].Replace("@", "").Trim();
            }

            if (string.IsNullOrEmpty(templateName)) {
                throw new Exception("No template declaration found. Usage example: @base will load template_base.txt.");
            }

            if (!lines[1].StartsWith("[") || !lines[1].EndsWith("]") ||
                lines[1].Contains(":")) {
                throw new Exception("No root class declaration found. Usage example: [Person] will create a Person root class.");
            }

            string rootDeclaration = lines[1].Replace("[", "").Replace("]", "").Trim();
            if (lines[1].Contains(",")) {
                var d = rootDeclaration.Split(',');
                rootNamespace = d[0].Trim();
                rootClassName = d[1].Trim();
            } else {
                rootNamespace = "NOT_SET";
                rootClassName = rootDeclaration;
            }

            string currentClassPath = rootClassName;

            Dictionary<string, List<Declaration>> declarations = new();
            declarations[currentClassPath] = new List<Declaration>();
            for (int i = 2; i < lines.Length; i++) {
                var line = lines[i];

                if (line.StartsWith('[') && line.EndsWith(']')) {
                    //new sub-class
                    currentClassPath = line.Replace("[", "").Replace("]", "").Trim();
                    declarations[currentClassPath] = new List<Declaration>();
                    continue;
                }

                Declaration dec = new();
                dec.parseDeclaration(line);

                declarations[currentClassPath].Add(dec);
            }

            rootClass = ClassStructure.BuildStructure(rootClassName, declarations);
        }
    }
}
