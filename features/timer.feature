Feature: Speed Cube Timer
  As a speedcuber
  I want to time my solves
  So that I can track my performance

  Background:
    Given the timer application is running
    And the timer is in ready state

  Scenario: Start timer with SPACE hold
    When I hold SPACE for 0.5 seconds
    Then the timer should start running
    And the status should show "Timer running..."

  Scenario: Stop timer with instant SPACE press
    Given the timer is running
    When I press and release SPACE quickly
    Then the timer should stop
    And the time should be recorded in the history
    And the timer display should reset to 00:00.00

  Scenario: Record solve time
    When I hold SPACE for 0.5 seconds
    And I wait 2.5 seconds
    And I press SPACE to stop
    Then a new time entry should appear in the solve history
    And the time should be approximately 2.5 seconds

  Scenario: View statistics
    Given I have recorded 3 solve times: 15.5s, 12.3s, 14.8s
    Then the best time should show 12.3s
    And the average time should show 14.2s
    And the worst time should show 15.5s

  Scenario: Select point from plot
    Given I have recorded multiple solves
    When I click on a point in the performance trend plot
    Then that solve should be highlighted in orange
    And the corresponding entry should be selected in the history list

  Scenario: Delete a solve
    Given I have recorded a solve time
    When I select the time in the history
    And I click the delete button
    Then the time should be removed from history
    And the plot should update without the deleted point
    And the statistics should recalculate

  Scenario: Reset session
    Given I have recorded multiple solve times
    When I press R to reset
    Then all history should be cleared
    And the plot should be empty
    And statistics should show no data
    And the timer display should reset to 00:00.00

  Scenario: Visual dark theme
    Then the application should display with black background
    And text should be white for readability
    And accent colors should be blue and orange
