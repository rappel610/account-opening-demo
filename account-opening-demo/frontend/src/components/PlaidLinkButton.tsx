import { usePlaidLink } from "react-plaid-link";

interface PlaidLinkButtonProps {
  linkToken: string;
  onSuccess: (publicToken: string) => void;
}

/**
 * Thin wrapper around react-plaid-link. Plaid Link is a hosted widget,
 * not something you build by hand, the integration work here is really
 * about the token handoff: get a link_token from the backend, hand it
 * to this widget, and hand the resulting public_token back to the
 * backend for exchange.
 */
export function PlaidLinkButton({ linkToken, onSuccess }: PlaidLinkButtonProps) {
  const { open, ready } = usePlaidLink({
    token: linkToken,
    onSuccess: (publicToken) => onSuccess(publicToken),
  });

  return (
    <button onClick={() => open()} disabled={!ready}>
      Connect a bank account
    </button>
  );
}
