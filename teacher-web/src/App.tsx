import { Hero } from './components/Hero'
import { ExperienceClassQuest } from './components/ExperienceClassQuest'
import { HowClassQuestWorks } from './components/HowClassQuestWorks'
import { ProblemSection } from './components/ProblemSection'
import { ClassQuestWorlds } from './components/ClassQuestWorlds'
import { FinalCtaFooter } from './components/FinalCtaFooter'

function App() {
  return (
    <main className="min-h-screen overflow-hidden bg-cq-background text-cq-text">
      <Hero />
      <ExperienceClassQuest />
      <ProblemSection />
      <HowClassQuestWorks />
      <ClassQuestWorlds />
      <FinalCtaFooter />
    </main>
  )
}

export default App
