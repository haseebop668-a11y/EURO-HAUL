# EURO HAUL — Economy, Player Companies & Payment Boundaries

## Product decision
EURO HAUL is a trucking simulation, not a bank, investment product, crypto exchange, or real-money wagering game. The initial economy has separate balances with distinct purposes and no conversion path between them.

## Currency model

### 1. Game Cash (earned gameplay currency)
- Earned from ordinary simulated freight contracts, delivery quality, milestones and company operations.
- Spent on simulated fuel, repairs, driver wages, garage rent, truck purchases/upgrades and company expansion.
- Has no cash value, cannot be withdrawn, transferred peer-to-peer, sold, or redeemed for fiat.
- It is a progression balance, not a deposit or financial account.

### 2. Premium Coins (purchased digital currency)
- Sold in clearly disclosed fixed bundles through Google Play Billing for the Play-distributed Android app, subject to current store policy and supported product configuration.
- Spent only inside EURO HAUL on disclosed digital goods such as cosmetics, garage decoration and optional convenience items.
- No cash redemption, peer-to-peer transfer, exchange listing or conversion into Game Cash.
- Purchases must be delivered from verified store purchase records; acknowledge purchases securely and restore eligible purchases.
- Price, quantity, region, tax treatment and terms must be clear before purchase. Do not sell randomized rewards without required odds disclosures and policy review.

### 3. Reward Tokens (optional earned rewards)
- Small non-transferable rewards from genuine completed rewarded ads or in-game milestones.
- Redeemable only for a defined set of digital items in EURO HAUL.
- No cash-out, transfer, trading or purchase-funded prize pool.

## Player-owned virtual logistics companies
Players may establish fictional in-game companies using Game Cash and progression:
- company name, logo, fleet, depot/garage and driver roster;
- contracts, operating costs, reputation, service quality and simulated profit/loss;
- later, optional cooperative contracts and player-to-player services, if anti-fraud and moderation are ready.

All balances and profits in this simulator remain fictional gameplay values. A company inside the game does not represent a registered real-world business or an ownership/security interest.

## Payment and developer revenue

### Android sales
For digital goods and virtual currency sold in a Google Play-distributed app, integrate Google Play Billing unless a documented policy exception/program applies. The purchased currency must remain within the game title. Do not add an external checkout link or payment diversion to the Play app unless the app is enrolled in an applicable Google program and meets its conditions.

### Developer payout access
- Configure the developer's genuine legal/payee identity and payments profile in Play Console.
- Add a bank account that meets Google's country-specific payout requirements.
- For Pakistan, Google's merchant bank-account instructions currently describe USD wire payouts from the United States and request bank name, account holder, SWIFT/BIC and IBAN details. Confirm the exact current requirements in the account and with the receiving bank before release.
- Review settlement reports, refunds, chargebacks, taxes, withholding, payment holds and bank fees.
- Ad revenue is separate: view finalized balances and transactions in AdMob Payments. Payment is monthly after the applicable threshold and holds are cleared.
- Never place passwords, bank details, tax identifiers, merchant credentials or SDK secrets in source control.

## Why player cash withdrawal is not part of the initial release
A feature that lets players convert gameplay balances, purchased Coins, ad rewards or game-performance rewards into PKR/USD creates real-money, payments, consumer-protection, tax, fraud and potentially gaming/financial-regulatory questions. It can also conflict with app-store rules. Google Play's real-money games policy generally restricts games that let users pay or stake money (including purchased in-app items) for a chance to obtain real-world monetary prizes; limited exceptions and loyalty-program rules have specific conditions.

Therefore:
- Do not build a cash-out button, fiat exchange rate, player payout wallet, peer-to-peer cash market, deposit pool or withdrawable token in the initial game.
- Do not describe Game Cash, Premium Coins or Reward Tokens as money, investment, income, stored value or redeemable assets.
- Never promise future cash redemption or imply a player has a payable balance.
- Do not make purchases a condition of winning a cash prize.
- Do not create a crypto/token/blockchain layer to bypass store or financial rules.

A future real-world payout concept may be researched only as a separate, gated product proposal after specialist legal review in every target country, written platform approval where required, and partnership with appropriately licensed payment providers. A compliant loyalty benefit, if available, would need to be genuinely supplementary to a qualifying transaction, use documented fixed ratios and meet the current store policy and local law. It must not be tied to wagering, chance, purchased game currency, or performance-based cash prizes. No implementation or launch date is promised.

## Server and ledger architecture (when online accounts are introduced)
Use an authoritative backend rather than trusting client-side wallet values:
- player_wallets: player ID, currency code, available balance, version.
- wallet_transactions: immutable transaction ID, player ID, currency, signed amount, reason, source reference, idempotency key, timestamp.
- store_purchases: store, product ID, purchase token/order ID, validation state, delivery state, refund state.
- reward_claims: provider transaction ID, verified completion, reward type/amount, idempotency key.
- company_accounts: player/company ID, Game Cash ledger references and progression state.
- Use database transactions, unique idempotency constraints, server-side purchase validation, refund/revocation handling, audit logs, rate limits and anomaly detection.
- Never accept client-submitted balance totals or trust a client-only ad completion flag.
- Keep currencies separate at the schema and API level; reject cross-currency transfers by design.

## Release gates
1. Legal entity, developer account, tax profile and bank payout setup verified.
2. Google Play Billing integration and test purchases verified.
3. Ad network account, category controls, consent, genuine reward callback and payout setup verified.
4. Terms, privacy policy, refund/support process and store disclosures published.
5. Backend wallet ledger and purchase/reward validation tested before real user purchases.
6. Cash-out remains disabled unless a separate approved legal/product review explicitly authorizes a compliant model.
