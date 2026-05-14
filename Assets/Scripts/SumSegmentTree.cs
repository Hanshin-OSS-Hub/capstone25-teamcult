using System;
using System.Collections.Generic;

public class SumSegmentTree
{
    private double[] tree = Array.Empty<double>();
    private int leafSize = 1;
    private int count = 0;

    /// <summary>
    /// 원본 데이터의 실제 원소 개수입니다.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// 세그먼트 트리 내부 리프 구간의 크기입니다.
    /// Count 이상인 가장 작은 2의 거듭제곱입니다.
    /// </summary>
    public int LeafSize => leafSize;

    /// <summary>
    /// 전체 구간의 합입니다.
    /// 세그먼트 트리 루트인 tree[1] 값을 반환합니다.
    /// </summary>
    public double TotalSum => tree.Length > 1 ? tree[1] : 0.0;

    public void Build(IReadOnlyList<double> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        count = values.Count;

        leafSize = 1;
        while (leafSize < count)
        {
            leafSize <<= 1;
        }

        tree = new double[leafSize << 1];

        for (int i = 0; i < count; i++)
        {
            tree[leafSize + i] = Math.Max(0.0, values[i]);
        }

        for (int i = leafSize - 1; i >= 1; i--)
        {
            tree[i] = tree[i << 1] + tree[(i << 1) + 1];
        }
    }

    public void SetValue(int index, double value)
    {
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));

        int node = leafSize + index;
        tree[node] = Math.Max(0.0, value);

        node >>= 1;

        while (node >= 1)
        {
            tree[node] = tree[node<<1] + tree[node<<1|1];
            node >>= 1;
        }
    }

    // [leftInclusive, rightExclusive) 구간 합
    public double RangeSum(int leftInclusive, int rightExclusive)
    {
        if (leftInclusive < 0 || rightExclusive < leftInclusive || rightExclusive > count)
            throw new ArgumentOutOfRangeException();

        double sum = 0.0;

        int left = leafSize + leftInclusive;
        int right = leafSize + rightExclusive;

        while (left < right)
        {
            if ((left & 1) == 1)
            {
                sum += tree[left];
                left++;
            }

            if ((right & 1) == 1)
            {
                right--;
                sum += tree[right];
            }

            left >>= 1;
            right >>= 1;
        }

        return sum;
    }

    // prefix sum 기준 이분탐색:
    // prefixSum(index) >= targetPrefixSum 을 만족하는 가장 작은 index를 반환합니다.
    public int LowerBoundByPrefixSum(double targetPrefixSum)
    {
        if (count == 0)
            return -1;

        if (targetPrefixSum < 0.0 || targetPrefixSum >= TotalSum)
            return -1;

        int node = 1;

        while (node < leafSize)
        {
            int left = node << 1;

            if (targetPrefixSum < tree[left])
            {
                node = left;
            }
            else
            {
                targetPrefixSum -= tree[left];
                node = left + 1;
            }
        }

        int index = node - leafSize;
        return index < count ? index : -1;
    }
}