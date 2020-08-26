// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

void CrestNodeAmbientLight_half
(
	out half3 o_ambientLight
)
{
	// Use the constant term (0th order) of SH stuff - this is the average
	o_ambientLight = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
#if !SHADERGRAPH_PREVIEW
	// Allows control of baked lighting through volume framework. X is indirect diffuse.
	o_ambientLight *= _IndirectDiffuseLightingMultiplier.x;
#endif
}
