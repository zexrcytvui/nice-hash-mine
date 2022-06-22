﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CommandLine
{
    //["--flagName"] // option is parameter

    //["--flagName", "value"] // option single parameter

    //["--zombie-tune", "value", ","] // option with multiple parameters - FOR DEVICES


    using Parameter = List<string>;
    using Parameters = List<List<string>>;
    using DevicesParametersList = List<List<List<string>>>;

    public static class MinerExtraParameters
    {
        public static ElpSettings ReadJson(string path)
        {
            var elp = JsonConvert.DeserializeObject<ElpSettings>(File.ReadAllText(path));
            return elp;
        }

        internal enum ParameterType
        {
            OptionIsParameter = 1,
            OptionWithSingleParameter = 2,
            OptionWithMultipleParameters = 3
        }

        internal static bool IsParameterOfType(Parameter parameter, ParameterType type) => parameter.Count == (int)type;
        private static Parameters FilterParametersOfType(Parameters parameters, ParameterType type)
        {
            return parameters.Where(p => IsParameterOfType(p, type)).ToList();
        }

        private static bool IsValidParameter(Parameter parameter)
        {
            foreach (ParameterType type in Enum.GetValues(typeof(ParameterType)))
            {
                if (IsParameterOfType(parameter, type)) return true;
            }

            return false;
        }

        private static bool CheckIfCanGroup(Parameter aP, Parameter bP, ParameterType type)
        {
            if (!IsParameterOfType(aP, type) || !IsParameterOfType(bP, type)) return false;
            if (aP[0] != bP[0]) return false;
            if (type == ParameterType.OptionWithSingleParameter && aP[1] != bP[1]) return false;
            if (type == ParameterType.OptionWithMultipleParameters && aP[2] != bP[2]) return false;

            return true;
        }

        private static bool CheckIfCanGroup(Parameter aP, Parameters b_Parameters, ParameterType type)
        {
            return b_Parameters.Count(bP => CheckIfCanGroup(aP, bP, type)) == 1;
        }

        private static bool CheckIfCanGroup(Parameters a, Parameters b, ParameterType type)
        {
            return a.All(aP => CheckIfCanGroup(aP, b, type)) && b.All(bP => CheckIfCanGroup(bP, a, type));
        }

        private static bool CheckIfCanGroup(Parameters a, Parameters b)
        {
            if (a == null || b == null) return false;

            foreach (ParameterType type in Enum.GetValues(typeof(ParameterType)))
            {
                var a_types = FilterParametersOfType(a, type);
                var b_types = FilterParametersOfType(b, type);
                if(!CheckIfCanGroup(a_types, b_types, type) && a_types.Count > 0 && b_types.Count > 0) return false;
            }

            return true;
        }
        public static bool CheckIfCanGroup(DevicesParametersList devicesParameters)
        {
            foreach (var a in devicesParameters)
            {
                foreach (var b in devicesParameters)
                {
                    if (a == b) continue;
                    if (!CheckIfCanGroup(a, b)) return false;
                }
            }
            return true;
        }

        private static string DevicesStringForFlag(string flag, IEnumerable<Parameters> parameters)
        {
            var flagParams = parameters
                .Select(p => p.FirstOrDefault(o => o[0] == flag));
            var delimiter = flagParams.FirstOrDefault()[2];
            var values = string.Join(delimiter, flagParams.Select(o => o[1]).ToArray());
            var mask = flag.EndsWith("=") ? "{0}{1}" : "{0} {1}";
            return string.Format(mask, flag, values);
        }

        public static string Parse(Parameters minerParameters, Parameters algorithmParameters, DevicesParametersList devicesParameters)
        {
            if (devicesParameters == null || devicesParameters.Count == 0 || minerParameters.Count == 0 || algorithmParameters.Count == 0) return "";
            if (!CheckIfCanGroup(devicesParameters)) return "";

            var options = FilterParametersOfType(devicesParameters.First(), ParameterType.OptionIsParameter).SelectMany(x => x).ToList();
            var singleOptions = FilterParametersOfType(devicesParameters.First(), ParameterType.OptionWithSingleParameter).SelectMany(x => x).ToList();
            var multipleOptions = devicesParameters
                .Select(p => FilterParametersOfType(p, ParameterType.OptionWithMultipleParameters));
            var ss = multipleOptions.SelectMany(p => p.Select(x => x[0]));
            var allFlags = new HashSet<string>(ss);
            var deviceFlagValues = allFlags.Select(flag => DevicesStringForFlag(flag, multipleOptions));
            var check = devicesParameters.SelectMany(x => x).ToList();

            options.AddRange(singleOptions);
            options.AddRange(deviceFlagValues);
            var algo = algorithmParameters.Where(p => check.All(o => o[0] != p[0]));
            var miner = minerParameters.Where(p => check.All(o => o[0] != p[0]) && algo.All(a=> a[0] != p[0]));
            var elp = "";
            if (miner.Any()) elp += string.Join(" ", miner.SelectMany(x => x));
            if (algo.Any()) elp += " " + string.Join(" ", algo.SelectMany(x => x));
            if (options.Any()) elp += " " + string.Join(" ", options);

            return elp;
        }
    }
}
