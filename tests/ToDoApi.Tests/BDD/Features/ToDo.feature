Feature: Todo Management

As an application user
I want to manage my todo items
So that I can keep track of my tasks

Scenario: Create a new todo
    Given the todo list is empty
    When I create a todo named "Learn .NET" with description "I want to learn .NET"
    Then the created todo should have name "Learn .NET"
    And the created todo should have description "I want to learn .NET"
    And the created todo status should be "Pending"

Scenario: Retrieve all todos
    Given a todo named "Learn .NET" exists
    When I retrieve all todos
    Then "one" todo should be returned

Scenario: Retrieve a todo by id
    Given a todo named "Learn .NET" exists
    When I retrieve that todo by id
    Then the returned todo should have the name "Learn .NET"

Scenario: Update a todo
    Given a todo named "Learn .NET" exists
    When I update its name to "Learn Reqnroll"
    Then the updated todo name should be "Learn Reqnroll"

Scenario: Delete a todo
    Given a todo named "Learn .NET" exists
    When I delete that todo
    Then the todo should no longer exist