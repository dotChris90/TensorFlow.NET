﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tensorflow
{
    public class Operation
    {
        private readonly IntPtr _handle;

        public Graph Graph { get; }
        public int _id => _id_value;
        private int _id_value;

        private Status status = new Status();

        public string name => c_api.TF_OperationName(_handle);
        public string optype => c_api.TF_OperationOpType(_handle);
        public string device => c_api.TF_OperationDevice(_handle);
        public int NumOutputs => c_api.TF_OperationNumOutputs(_handle);
        public TF_DataType OutputType => c_api.TF_OperationOutputType(new TF_Output(_handle, 0));
        public int OutputListLength => c_api.TF_OperationOutputListLength(_handle, "output", status);
        public int NumInputs => c_api.TF_OperationNumInputs(_handle);
        public int NumConsumers => c_api.TF_OperationOutputNumConsumers(new TF_Output(_handle, 0));
        public int NumControlInputs => c_api.TF_OperationNumControlInputs(_handle);
        public int NumControlOutputs => c_api.TF_OperationNumControlOutputs(_handle);

        private Tensor[] _outputs;
        public Tensor[] outputs => _outputs;
        public Tensor[] inputs;

        public Operation(IntPtr handle)
        {
            _handle = handle;
        }

        public Operation(Graph g, string opType, string oper_name)
        {
            Graph = g;

            var desc = c_api.TF_NewOperation(g, opType, oper_name);
            c_api.TF_SetAttrType(desc, "dtype", TF_DataType.TF_INT32);
            c_api.TF_FinishOperation(desc, status);
        }

        public Operation(NodeDef node_def, Graph g, List<Tensor> inputs = null, TF_DataType[] output_types = null, object control_inputs = null, TF_DataType[] input_types = null, string original_op = "", OpDef op_def = null)
        {
            Graph = g;

            _id_value = Graph._next_id();
            _handle = ops._create_c_op(g, node_def, inputs);

            _outputs = new Tensor[NumOutputs];
            for (int i = 0; i < NumOutputs; i++)
            {
                _outputs[i] = new Tensor(this, i, output_types[i]);
            }

            Graph._add_op(this);
        }

        public object get_attr(string name)
        {
            object ret = null;

            var fields = new string[] { "s", "i", "f", "b", "type", "shape", "tensor", "func" };

            switch (name)
            {
                case "dtype":
                    ret = _outputs[0];
                    break;
                case "shape":
                    ret = new TensorShapeProto();
                    break;
            }

            return ret;
        }

        public static implicit operator Operation(IntPtr handle)
        {
            return new Operation(handle);
        }

        public static implicit operator IntPtr(Operation op)
        {
            return op._handle;
        }
    }
}
