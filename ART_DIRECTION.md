# Sunset Leap — MVP art direction

Original fictional South Asian city at sunset. Teal jacket, orange scarf, dark trousers, warm window light, indigo rooftops and a coral sky. The supplied concept is a mood reference; generated assets are new illustrations, not extracted from it.

## Asset production

Created with the built-in image generation tool. Final project assets live in `Assets/RooftopRunner/Art/Sunset/`. Original PNG sheets are retained; Unity sprite rectangles select usable regions without modifying source pixels. The runner background received an additional transparency cleanup pass.

The sky and city strip produce three scenery layers: fixed sky, distant city and nearer city. The two city layers reuse one texture with different scale, tint and scrolling speed. Mirrored repeat tiles avoid abrupt edge seams. There are three facade styles, eight decorative props and eight character poses (four run frames plus charge, rising, falling and landing). Animation is intentionally a small MVP sprite cycle, not a polished hand-animated production rig.

Props are visual only. Existing collision boxes and jump physics are retained. No trajectory or landing marker is enabled. UI text, score and charge indicators remain live Unity elements.

## Generation prompts

### sky

Use case: stylized-concept. Asset type: production background texture for a 2D side-scrolling Unity game, wide landscape 1536x1024. Paint an original cinematic South Asian summer sunset SKY ONLY. Rich coral orange horizon, golden peach center, dusty plum upper sky, long softly painted violet clouds. Beautiful hand-painted animation background with layered brush texture, not photoreal. Sun is a pale golden disc toward right side around 70% x and 60% y. Full bleed sky all the way to bottom. NO city, buildings, ground, objects, text, UI, borders, characters. Calm cloud detail with readable open areas. Warm sunset reference mood.

### city

Wide landscape transparent parallax skyline sprite for 2D game. Original fictional South Asian dense city, hand-painted cinematic sunset style. Muted dusty mauve purple layered flat roof houses, few slender towers, domes and minarets, tiny warm windows. Buildings fill bottom 65%, irregular skyline, transparent above; opaque buildings reach bottom and both side edges at similar heights for scrolling. Straight side view. No sky, sun, ground, text, UI, foreground platforms or people. Actual transparency.

### roofs

Production sprite atlas on transparent background, EXACT THREE columns and ONE row. Three isolated equally sized rectangular South Asian building facade modules with transparent gutters. Straight FRONT elevation, no perspective, no side walls. All have perfectly straight flat horizontal roof tops and vertical rectangular sides. Dark painterly cinematic sunset illustration, indigo charcoal weathered concrete, thin amber rim on roof cornice, warm glowing grilled windows, pipes and cables. Left grey concrete two windows, middle dusty plum plaster balcony and two windows, right muted teal shutters and two windows. No rooftop props, no sky, no ground, no people, no labels. Each facade is taller than wide. All three full rectangles same top/bottom level. Actual transparency around each rectangle.

### props

Transparent sprite atlas EXACT 4 columns by 2 rows with eight fully isolated objects, equal cells and generous gutters. Row1 left to right: blue cylindrical rooftop water tank; TV antenna on stand; terracotta pot leafy plant; small satellite dish. Row2: clothesline with two wooden posts and three cloths; compact rooftop doorway shack with flat awning and amber lamp; outdoor air conditioning unit; patterned rug draped over short rail. Straight-on side-view South Asian rooftop game props, richly hand-painted cel illustration, indigo shadows, orange sunset rim light. Every object complete within its own cell, no overlap. No grid lines, ground, sky, shadows outside object, text, people or background. Actual transparency.

### runner

Transparent 2D animation sprite sheet, EXACT 4 columns by 2 rows uniform grid, eight full-body poses of SAME South Asian young adult male runner facing RIGHT. Teal jacket, cream undershirt, charcoal trousers, cream sneakers, short messy black hair, orange scarf trailing LEFT. Clean hand-painted cel game illustration, bold readable silhouette, slim athletic proportions, warm rim light. Identical character scale in every cell, ample transparent gutters, every figure complete, no overlap. Row1 four successive run cycle frames: contact, passing, opposite contact, opposite passing. Row2: crouched charging anticipation; ascending jump knees bent; descending fall feet down; crouched landing. Strict profile facing right all frames. Consistent clothing and identity. No floor, shadows, effects, text, grid lines or background. Actual transparency.

### Runner transparency cleanup

Edit this exact runner sprite sheet: remove ONLY the entire brown/orange smoky background and ALL background glow, replacing it with truly transparent pixels (alpha 0). Preserve the eight runner figures, their clothing colors, orange scarves, poses, scale, cell positions, faces, and every character detail exactly. Do not redraw, move, resize, add or remove figures. Clean precise cutouts around hair, scarves, hands, shoes and between limbs. No ground, shadows or colored haze outside the characters. Output must be a transparent PNG sprite sheet.


## Editing in Unity

- Open `Assets/RooftopRunner/Scenes/RooftopRunner.unity` and press Play.
- Runner → RunnerAnimator: adjust visual height and run-frame rate. Collision size remains on BoxCollider2D.
- Sunset City → CityBackdrop: adjust distant/near scrolling speeds.
- Rooftop prefab → RooftopDress: shared art reference. Prop heights and placements are defined in the small component. Props have no colliders.
- Art/Sunset/SunsetArt.asset: source textures and sprite rectangles; frame order is run 0–3, charge 4, jump 5, fall 6, land 7.
- Rebuilding the MVP scene regenerates the import regions and layout; preserve custom changes before doing so.

This milestone targets a readable landscape Android presentation, but no specific phone performance guarantee is made before device testing. No paid art services, external asset downloads or audio purchases were used.
