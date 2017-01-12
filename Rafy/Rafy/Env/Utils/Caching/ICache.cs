/*******************************************************
 * 
 * ���ߣ��ξ���
 * ����ʱ�䣺20170112
 * ˵����ͨ�û������еĽӿ�
 * ���л�����.NET 4.0
 * �汾�ţ�1.0.0
 * 
 * ��ʷ��¼��
 * �����ļ� �ξ��� 20170112 -- �ع� Caching ģ�顣
 * 
*******************************************************/

using System;

namespace Rafy.Utils.Caching
{
    public interface ICache
    {
        /// <summary>
        /// �Ƿ�򿪻��湦�ܡ�
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// ͨ�� <paramref name="key"/> �� <paramref name="region"/> �ӻ����л�ȡ�����
        /// ��������ڣ��򷵻�null��
        /// </summary>
        /// <param name="key">��ʾһ������ļ���</param>
        /// <param name="region">��ʾ������������</param>
        /// <returns>���ػ����</returns>
        object Get(string key, string region = null);

        /// <summary>
        /// �򻺴������һ�������
        /// </summary>
        /// <param name="key">��ʾһ������ļ���</param>
        /// <param name="value">��ʾһ�������ֵ��</param>
        /// <param name="policy">����ʹ�õĲ��ԣ�һ���ǹ��ڲ��ԣ�</param>
        /// <param name="region">��ʾ������������</param>
        /// <returns>��ӳɹ����� true, ʧ�ܷ��� false��</returns>
        bool Add(string key, object value, Policy policy, string region = null);

        /// <summary>
        /// ��ָ����ӻ������Ƴ���
        /// </summary>
        /// <param name="key">��ʾһ������ļ���</param>
        /// <param name="region">��ʾ������������</param>
        void Remove(string key, string region = null);

        /// <summary>
        /// ɾ��ĳ�������е�����ֵ��
        /// </summary>
        /// <param name="region">��ʾ������������</param>
        void ClearRegion(string region);

        /// <summary>
        /// ������л��档
        /// </summary>
        void Clear();

        /// <summary>
        /// �ӻ����л�ȡָ����ֵ��
        /// <para>����ʹ�û����ȡ����������ڣ������ifNotExists������ȡ����ֵ������ӵ������С�</para>
        /// </summary>
        /// <typeparam name="T">��ʾ����������͡�</typeparam>
        /// <param name="key">��ʾһ������ļ���</param>
        /// <param name="ifNotExists">��ʾ���δȡ�����棬ִ�е�ί�С�</param>
        /// <param name="regionName">��ʾ������������</param>
        /// <param name="policy">��ʾ������ԡ�</param>
        /// <returns></returns>
        T Get<T>(string key, Func<T> ifNotExists, string regionName = null, Policy policy = null) where T : class;
    }
}