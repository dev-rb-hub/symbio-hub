# ADR 0004: Cryptography, key management and user authentication

- **Status:** Proposed
- **Date:** 2026-10-15

## Context

The community MVP hashes passwords and signs session tokens with locally configured secrets. That is acceptable for demos but not for regulated or high-trust hosted editions. Regulated buyers expect platform-maintained cryptography, protected keys, unique per-environment secrets and modern authentication. Writing our own cryptographic constructions is a risk we do not need to take.

## Decision

1. **No custom cryptography.** Use the platform and framework implementations: .NET `System.Security.Cryptography` (backed by the operating system's cryptographic provider) and the ASP.NET Core Identity password hasher (versioned, salted PBKDF2-HMAC-SHA512). Local code may adapt or migrate formats but must not define its own algorithms. A third-party library such as BouncyCastle is acceptable only where the platform lacks a needed algorithm. Note that the standard open-source BouncyCastle distribution is not a FIPS-validated module. The Bouncy Castle project also offers separate FIPS-certified library editions; these are distinct products with their own certificate, scope, approved-mode constraints and licensing, so confirm the current certificate and the edition and language in use before relying on one or making any claim. Algorithms such as Argon2id are not FIPS-approved, so FIPS-constrained editions stay with approved algorithms such as PBKDF2.
2. **Key management behind a port.** Add a provider-neutral key and signing port in `Symbio.Core` (sign, verify, HMAC, wrap and unwrap). Adapters in `Symbio.Infrastructure` are selected by configuration:
   - Community: secrets supplied through environment variables or a secret store, never committed to source.
   - Hosted and regulated editions: a managed key vault or HSM (for example Azure Key Vault or Managed HSM, AWS KMS or CloudHSM, or an on-premises PKCS#11 device). In high-trust configurations keys are non-exportable and signing and HMAC operations run inside the HSM.
   - Validation levels (for example FIPS 140-3) differ by product, tier and region. Confirm the current certificate and scope with the vendor before making any claim.
3. **Secrets versus parameters.** Format identifiers and algorithm parameters (such as the password hash version or iteration count) are not secrets. They are versioned, set by configuration with a security floor, and upgraded transparently at login. Secrets (signing keys, webhook secrets, any password pepper) are never hardcoded and come from the key management port.
4. **Optional password pepper.** High-trust editions may apply an HMAC with a vault- or HSM-held key before password hashing so a database leak alone is insufficient to attack hashes.
5. **Token signing.** Move from a shared symmetric signing key to asymmetric keys held in the key management port, with key identifiers and rotation.
6. **Seed data.** Demo accounts exist only in Development or by explicit opt-in. Their password comes from configuration, there is no default outside Development, and every stored password hash has a unique random salt. Per-environment secrets must be unique and must not be shared between environments.
7. **Modern authentication.** Hosted editions delegate sign-in to an OIDC identity provider that supplies MFA and passkeys (WebAuthn/FIDO2), for example Microsoft Entra ID, Entra External ID or Microsoft accounts, extending [ADR 0003](0003-identity-assertion-ingestion.md). Local passwords remain for the community edition. If passkeys are needed without an external provider, use a maintained library or the framework's built-in support, not a custom WebAuthn implementation.
8. **Payments scope.** The platform does not store, process or transmit card data. Card handling stays with the payment provider through tokenisation, keeping PCI DSS scope minimal. If a deployment ever brings cardholder data into scope, the relevant PCI DSS key management, transmission and authentication requirements apply and an assessor must be engaged.

9. **Encoding, delimiters and cross-protocol messages.** Ciphertext, signatures, salts, keys and hashes are arbitrary bytes and can contain any character, so they must never be joined with, or parsed using, text delimiters unless first encoded to a standard alphabet (Base64 or Base64url, or hex), and the encoding must be fixed and stated per field.
   - Prefer a framework- or standard-defined container (the Identity hash format, JOSE/JWT, COSE, CMS) over a locally invented delimited string. Where a composite value is unavoidable, use length-prefixed or structured encoding (for example JSON or CBOR with an explicit version) and validate every field strictly on read.
   - Sign or MAC the exact bytes received, not a re-serialised form, and bind every field that affects meaning (version, algorithm, key identifier, timestamp, context). Concatenating variable-length fields before signing needs an unambiguous encoding so that different field splits cannot produce the same signed bytes. Compare secrets and MACs in constant time.
   - Messages cross protocol boundaries (for example SOAP, REST, queues, webhooks). Each hop can change character encoding, whitespace, line endings, XML canonicalisation, JSON key order or Base64 variant. Do not verify a signature after a hop that may have altered the bytes. Verify at the boundary where the signed bytes are intact, or use a signature designed for the format (for example XML-DSig for SOAP, or HTTP message signatures or JWS for REST). Define the canonical form and encoding in the contract for each integration, and add tests that round-trip representative payloads, including delimiter characters, non-ASCII text and binary data, across each hop.

## Consequences

- **Implemented now:** framework password hasher with salted PBKDF2, configurable iteration count with a floor, transparent upgrade of legacy hashes, and demo seed accounts gated to Development.
- **Planned:** key management port and adapters, asymmetric token signing, optional pepper, OIDC sign-in with MFA and passkeys.
- No claim of FIPS validation, PCI DSS compliance or HSM protection is made for the current code. Those apply only to a configured deployment that has been assessed.
- New or changed integrations that sign, encrypt or hash data must document their encoding and canonical form, and are reviewed against point 9.
- Key and trust configuration becomes security-sensitive and must be audited and rotated.
