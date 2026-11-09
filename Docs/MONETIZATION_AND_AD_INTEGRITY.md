# EURO HAUL — Monetization, Rewarded Ads & Ad Integrity

## Product goals
- Monetize without interrupting driving or pressuring players.
- Offer optional rewarded ads with a clear, disclosed in-game reward.
- Keep an ad-free play path.
- Record only genuine SDK-reported ad events and verified rewards.
- Prefer family-safe inventory and direct sponsorships from screened, relevant brands.

## Player-facing placements
1. Reward garage: optional rewarded ad for a small, capped fuel voucher or soft-currency bonus.
2. Post-delivery results: optional reward offer after the result screen, never automatically played.
3. Cosmetic preview: optional rewarded ad to temporarily preview a paint/decoration.
4. No ads during driving, loading/unloading, toll approach, tutorials, menus with active decisions, or critical gameplay.
5. No forced interstitials in the initial release. If ever introduced, they must be explicitly enabled, infrequent, and shown only between completed activities.

## Halal-focused inventory policy
Use the selected network's current content-rating, category-exclusion, advertiser/block-list and family-safe inventory controls. At minimum, configure exclusions for adult/sexual content, gambling, alcohol, tobacco/nicotine and dating where the network supports them. Review creatives and block unsuitable advertisers/categories where available. Prefer direct sponsorships with written creative approval and a manual review process.

Important limitation: a third-party ad network can change inventory and category labels. App-side settings cannot guarantee that every served creative is halal or appropriate. If strict content certainty is required, disable open-exchange ads and use only manually reviewed direct sponsorships or a fully curated ad provider.

## Reward and accounting integrity
- A reward is granted only after a genuine SDK completion callback and, where supported, server-side verification (SSV).
- Use a unique transaction/reward ID and reject duplicate callbacks.
- Never simulate impressions, clicks, completions, installs, or view time.
- Never run hidden ads, background ads, auto-refresh loops, or ads invisible to players.
- Keep analytics separate from the player's wallet ledger.
- Record provider-reported impressions/revenue; label estimates as estimates until payout is confirmed.
- Respect consent, age requirements, privacy law, Google Play policy and each provider's terms.
- Do not reward ad clicks. Reward only eligible completed views when the provider permits it.

## Revenue model
1. Optional rewarded ads (primary ad format).
2. Cosmetic-only purchases or supporter pack, if later implemented and policy-compliant.
3. Screened direct sponsorships (fictional logistics/depot placements or approved brand placements).
4. No pay-to-win progression and no forced ad gates for core contracts.

## Implementation status
The initial Unity code adds:
- A ScriptableObject policy with conservative frequency caps and category intent flags.
- A provider interface so the game is not coupled to one ad SDK.
- Explicit consent and driving-state gates.
- Optional player-initiated rewarded ads.
- Reward grant gating on verified completion and transaction ID deduplication.

Not yet implemented:
- A real SDK adapter, dashboard category settings, consent-management platform, backend SSV endpoint, wallet persistence, analytics export, payout reconciliation or legal review.
- The policy flags are not a substitute for provider dashboard exclusions.
- No ad network has been selected or integrated, and no revenue is being generated yet.

## Release gates
Before enabling ads:
1. Select a supported provider and confirm availability for the target country/platform.
2. Review halal/content suitability and configure dashboard-level exclusions.
3. Implement consent and age handling, privacy disclosures and data-safety declarations.
4. Implement provider adapter and server-side reward verification.
5. Test no-fill, offline, timeout, duplicate callback, app pause/resume and denied-consent cases.
6. Verify store and provider policies and payout eligibility.
7. Keep ads disabled until all checks pass.
