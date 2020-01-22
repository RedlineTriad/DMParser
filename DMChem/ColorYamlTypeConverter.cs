using System;
using System.Drawing;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DMChem
{
    internal sealed class ColorYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Color);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var node = (Color)value;
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            emitter.Emit(new Scalar(null, nameof(node.R)));
            emitter.Emit(new Scalar(null, node.R.ToString()));
            emitter.Emit(new Scalar(null, nameof(node.G)));
            emitter.Emit(new Scalar(null, node.G.ToString()));
            emitter.Emit(new Scalar(null, nameof(node.B)));
            emitter.Emit(new Scalar(null, node.B.ToString()));
            if (node.A != byte.MaxValue)
            {
                emitter.Emit(new Scalar(null, nameof(node.A)));
                emitter.Emit(new Scalar(null, node.A.ToString()));
            }
            emitter.Emit(new MappingEnd());
        }
    }
}