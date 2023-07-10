float4 DistanceLighting(float4 currentColor, float3 position, float3 lightPosition)
{
    const int darknessDistance = 7;
    const float dist = saturate(distance(position, lightPosition) / darknessDistance);
    const float distSquared = dist * dist;
    currentColor.rgb = lerp(currentColor.rgb, currentColor.rgb * 0.35, distSquared);
    return currentColor;
}

// const float dotProduct = saturate(dot(float3(0, 1, 0), input.Normal));
// outputColor.rgb = lerp(outputColor.rgb * 0.8, outputColor.rgb, dotProduct);