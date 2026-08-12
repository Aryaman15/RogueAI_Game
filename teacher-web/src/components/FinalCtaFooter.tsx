import { ArrowRight, Compass, Sparkles } from 'lucide-react'

import './final-cta-footer.css'

const footerLinks = ['Product', 'Worlds', 'For Teachers', 'Insights'] as const

export function FinalCtaFooter() {
  return (
    <section aria-labelledby="final-cta-title" className="cq-final">
      <div className="mx-auto w-full max-w-7xl px-6 pt-20 sm:px-10 lg:px-14 lg:pt-24">
        <div className="cq-final-cta">
          <div className="cq-final-light" aria-hidden="true" />
          <div className="cq-final-content">
            <p className="cq-section-eyebrow">Start the next mission</p>
            <h2 id="final-cta-title">
              Your next assignment doesn't have to feel like homework.
            </h2>
            <p className="cq-final-rhythm">
              Build it.
              <br />
              Play it.
              <br />
              Understand it.
            </p>
            <div className="cq-final-actions">
              <button className="cq-button cq-button-primary" type="button">
                Create a mission
                <ArrowRight aria-hidden="true" className="size-4" />
              </button>
              <button className="cq-button cq-button-secondary" type="button">
                <Compass aria-hidden="true" className="size-4" />
                Explore worlds
              </button>
            </div>
          </div>
        </div>

        <footer className="cq-footer">
          <div>
            <a className="cq-footer-brand" href="#top" aria-label="ClassQuest home">
              <Sparkles aria-hidden="true" className="size-4" />
              ClassQuest
            </a>
            <p>
              Turn assignments into adventures.
              <br />
              Understand how students learn.
            </p>
          </div>

          <nav aria-label="Footer navigation" className="cq-footer-links">
            {footerLinks.map((link) => (
              <a href="#top" key={link}>
                {link}
              </a>
            ))}
          </nav>
        </footer>
      </div>
    </section>
  )
}
