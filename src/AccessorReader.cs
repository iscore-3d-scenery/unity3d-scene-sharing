using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using glTFLoader.Schema;

public class AccessorReader {
    private static Dictionary<Accessor.ComponentTypeEnum, int> s_AccessorComponentTypeEnumSize = new Dictionary<Accessor.ComponentTypeEnum, int>()
    {
        {Accessor.ComponentTypeEnum.BYTE,           sizeof(System.SByte)},
        {Accessor.ComponentTypeEnum.UNSIGNED_BYTE,  sizeof(System.Byte)},
        {Accessor.ComponentTypeEnum.FLOAT,          sizeof(System.Single)},
        {Accessor.ComponentTypeEnum.SHORT,          sizeof(System.Int16)},
        {Accessor.ComponentTypeEnum.UNSIGNED_SHORT, sizeof(System.UInt16)},
    };

    private static Dictionary<Accessor.TypeEnum, int> s_AccessorTypeEnumSize = new Dictionary<Accessor.TypeEnum, int>()
    {
        {Accessor.TypeEnum.MAT2,   4},
        {Accessor.TypeEnum.MAT3,   9},
        {Accessor.TypeEnum.MAT4,   16},
        {Accessor.TypeEnum.SCALAR, 1},
        {Accessor.TypeEnum.VEC2,   2},
        {Accessor.TypeEnum.VEC3,   3},
        {Accessor.TypeEnum.VEC4,   4},
    };

    private Accessor m_accessor;
    private byte[] m_data;
    private int m_offset;
    private int m_skip;
    private int m_componentSize;
    private System.Func<float> m_readComponent;

    public AccessorReader(Gltf model, string accessorID)
    {
        m_accessor = model.Accessors[accessorID];
        Bufferview bufferView = model.BufferViews[m_accessor.BufferView];
        Buffer buffer = model.Buffers[bufferView.Buffer];
        m_data = buffer.Uri;

        m_offset = bufferView.ByteOffset + m_accessor.ByteOffset;
        m_componentSize = s_AccessorComponentTypeEnumSize[m_accessor.ComponentType];
        m_readComponent = ReadComponentFunc();

        int elementSize = s_AccessorTypeEnumSize[m_accessor.Type] * m_componentSize;
        if (m_accessor.ByteStride <= 0)
            m_skip = 0;
        else
            m_skip = m_accessor.ByteStride - elementSize;
        
        int sizeToRead = m_accessor.Count * m_accessor.ByteStride;
        int sizeReadable = buffer.ByteLength - m_offset;
        if (sizeReadable < sizeToRead)
            throw new System.IndexOutOfRangeException("Not enough elements: " + sizeReadable + " < " + sizeToRead +
                " [Buffer length=" + buffer.ByteLength + " ]" +
                " [BufferView offset=" + bufferView.ByteOffset + " ]" +
                " [Accessor offset=" + m_accessor.ByteOffset + " stride=" + m_accessor.ByteStride + " count=" + m_accessor.Count + " ]" +
                " ComponentSize=" + m_componentSize + " TypeSize=" + s_AccessorTypeEnumSize[m_accessor.Type]);
    }

    public int[] ReadIntArray()
    {
        System.Func<int> read = () => (int) ReadComponent();
        return ReadTypeArray(read, Accessor.TypeEnum.SCALAR);
    }

    public Vector4[] ReadVector4Array()
    {
        System.Func<Vector4> read = () => new Vector4(ReadComponent(), ReadComponent(), ReadComponent(), ReadComponent());
        return ReadTypeArray(read, Accessor.TypeEnum.VEC4);
    }

    public Vector3[] ReadVector3Array()
    {
        System.Func<Vector3> read = () => new Vector3(ReadComponent(), ReadComponent(), ReadComponent());
        return ReadTypeArray(read, Accessor.TypeEnum.VEC3);
    }

    public Vector2[] ReadVector2Array()
    {
        System.Func<Vector2> read = () => new Vector2(ReadComponent(), ReadComponent());
        return ReadTypeArray(read, Accessor.TypeEnum.VEC2);
    }

    private T[] ReadTypeArray<T>(System.Func<T> read, Accessor.TypeEnum expectedType)
    {
        if (m_accessor.Type != expectedType)
            throw new System.TypeLoadException("Wrong expected type: " + expectedType + " got " + m_accessor.Type);
        T[] res = new T[m_accessor.Count];
        for (int i = 0; i < m_accessor.Count; ++i)
        {
            res[i] = read();
            m_offset += m_skip;
        }
        return res;
    }

    private System.Func<float> ReadComponentFunc()
    {
        switch (m_accessor.ComponentType)
        {
            case Accessor.ComponentTypeEnum.BYTE:
                return () => (System.SByte) m_data[m_offset];
            case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                return () => (System.Byte)  m_data[m_offset];
            case Accessor.ComponentTypeEnum.SHORT:
                return () => System.BitConverter.ToInt16(m_data, m_offset);
            case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                return () => System.BitConverter.ToUInt16(m_data, m_offset);
            case Accessor.ComponentTypeEnum.FLOAT:
                return () => System.BitConverter.ToSingle(m_data, m_offset);
        }
        throw new System.NotImplementedException("Unknow component type");
    }

    private float ReadComponent()
    {
        float res = m_readComponent();
        m_offset += m_componentSize;
        return res;
    }
}
