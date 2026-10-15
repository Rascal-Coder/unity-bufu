# Coffee UI Particle

Vendored unchanged Runtime and Shaders from the original upstream release **4.13.0**:
https://github.com/mob-sakai/ParticleEffectForUGUI/tree/4.13.0

MIT license retained in LICENSE.md; upstream package metadata retained in upstream-package.json.

Downloaded release archive SHA256:
`7f948e87de28c3275d3c37fe78e33f1faa7252c89326486a7867e894d6381f91`

The upstream Runtime includes its own Coffee.UIParticle.R.dll player compatibility bridge, preserved with the original import metadata. No simulation, material, or renderer code has been rewritten.

The game's extracted UIParticle serialization uses the same Coffee component fields. Original ParticleSystem modules are restored into native prefabs by NativeParticleBuilder, including curves, UV animation, and noise, and rendered through this library.
