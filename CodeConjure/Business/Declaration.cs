﻿namespace CodeConjure
{
	internal class Declaration
	{
		internal string name;
		internal string type;
		internal bool isStatic;
		internal bool isPrivate;
		internal bool isGetter;
		internal bool isSetter;
		internal List<Declaration>? functionParams;
		internal string? functionReturnType;
		internal bool isConstructor;
		internal string comment;
		private bool _isFunction;
		internal List<string> extras;

		internal bool isFunction
		{
			get
			{
				return _isFunction;
			}
			set
			{
				_isFunction = value;

				if (value)
				{
					functionParams = [];
					functionReturnType = "null";
				}
				else
				{
					functionParams = null;
					functionReturnType = null;
				}
			}
		}

		internal bool isProperty
		{
			get
			{
				return isSetter || isGetter;
			}
		}

		internal Declaration()
		{
			extras = [];
			name = "";
			type = "";
			comment = "";
			isStatic = false;
			isPrivate = false;
			isConstructor = false;
			isSetter = false;
			isGetter = false;
			_isFunction = false;
		}

		internal void parseDeclaration(string line)
		{
			if (!line.Contains(':'))
			{
				throw new Exception("Invalid line format: " + line);
			}

			int lastSplit = line.LastIndexOf(':');
			string key = line[..lastSplit].Trim();
			string val = line[(lastSplit + 1)..].Trim();

			if (key.Contains('(') && key.Contains(')'))
			{
				this.isFunction = true;
				this.functionParams = [];

				string functionName = key;
				if (key.StartsWith('('))
				{
					this.isConstructor = true;
					functionName = "";
				}
				else
				{
					functionName = key[..key.IndexOf('(')];
				}

				string[] rawParams = Utils.GetValueBetweenBrackets(key).Split(',')
					.Select(x => x.Trim()).ToArray();

				foreach (string rawParam in rawParams)
				{
					if (string.IsNullOrEmpty(rawParam))
					{
						continue;
					}

					Utils.ValidateFunctionParam(rawParam);

					Declaration param = new();
					param.parseDeclaration(rawParam);
					this.functionParams.Add(param);
				}

				key = functionName;
			}

			if (key.StartsWith('&'))
			{
				this.isStatic = true;
				key = key[1..];
			}

			if (!this.isConstructor && (char.IsLower(key[0]) || key[0] == '_'))
			{
				this.isPrivate = true;
				if (this.isFunction)
				{
					key = key.Replace('_', ' ').Trim();
				}
			}

			if (val.Contains("//"))
			{
				this.comment = val[val.IndexOf("//")..].Replace("//", "").Trim();
				val = val[..val.IndexOf("//")].Trim();
			}

			this.name = key;
			if (this.isFunction)
			{
				this.functionReturnType = val;
			}
			else
			{
				this.type = val;
			}
		}
	}
}