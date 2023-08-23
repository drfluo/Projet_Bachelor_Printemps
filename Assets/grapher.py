import pandas as pd
import matplotlib.pyplot as plt

input_file = "Result.txt"

# Read the content of the input file
with open(input_file, 'r') as f:
    content = f.readlines()

# Initialize variables to store extracted data
individual_results_data = []

# Flag to indicate when to start collecting individual car results
collect_individual_results = False

# Process each line in the content
for line in content:
    if "Individual cars results" in line:
        collect_individual_results = True
    elif "Total Results" in line:
        collect_individual_results = False
    elif collect_individual_results and "(" in line:
        entry = line.strip().split('/')
        individual_results_data.append(entry)

# Define column names
column_names = [
    "MapUsed", "CarLoad", "Started", "Arrived", "TimeTraveled", "NbStopped",
    "TimeStopped", "TimeFullSpeed", "RespectStop - NbStopDisrepected",
    "Path Choosen", "HighReactionTime", "Speed"
]

# Create a pandas DataFrame
df = pd.DataFrame(individual_results_data, columns=column_names)


pd.set_option('display.max_columns', None)
pd.set_option('display.max_rows', None)

# Print the DataFrame without truncation
print(df)

df['TimeTraveled'] = df['TimeTraveled'].str.replace(',', '').astype(float)



df2 = df[df['MapUsed'] == 'hugeMapNoLight.json']

# Calculate the average time traveled and include other columns for rows with the same "Started" and "Arrived" values
grouped_data = df2.groupby(['Started', 'Arrived', 'CarLoad']).agg({
    'MapUsed': 'first', # Use 'first' to take the value of the first occurrence
    'Speed': 'first',
    'TimeTraveled': 'mean'
}).reset_index()

# Plotting
plt.figure(figsize=(12, 12))  # Adjust the figure size as needed

# Loop through each group and plot
for group_name, group_data in grouped_data.groupby(['Started', 'Arrived']):
    car_load = group_data['CarLoad'].to_numpy()
    time_traveled = group_data['TimeTraveled'].to_numpy()
    
    # Change the legends here
    custom_legend_name = f"{group_name[0]} to {group_name[1]}"  # Custom name based on Started and Arrived
    plt.scatter(car_load, time_traveled, label=custom_legend_name)

    plt.plot(car_load, time_traveled, marker='o', label='_nolegend_')
plt.xlabel('Car Load')
plt.ylabel('Average Time Traveled')
plt.title('Time Traveled vs. Car Load \nWithout Light')

plt.ylim(ymin=0, ymax=300)


# Adjust the legend position and font size
plt.legend(bbox_to_anchor=(1.1, 1), loc='upper left', prop={'size': 8})

plt.grid()
plt.show()



df3 = df[df['MapUsed'] == 'hugeMapWithLight.json']


# Calculate the average time traveled and include other columns for rows with the same "Started" and "Arrived" values
grouped_data = df3.groupby(['Started', 'Arrived', 'CarLoad']).agg({
    'MapUsed': 'first', # Use 'first' to take the value of the first occurrence
    'Speed': 'first',
    'TimeTraveled': 'mean'
}).reset_index()

# Plotting
plt.figure(figsize=(12, 12))  # Adjust the figure size as needed

# Loop through each group and plot
for group_name, group_data in grouped_data.groupby(['Started', 'Arrived']):
    car_load = group_data['CarLoad'].to_numpy()
    time_traveled = group_data['TimeTraveled'].to_numpy()
    
    # Change the legends here
    custom_legend_name = f"{group_name[0]} to {group_name[1]}"  # Custom name based on Started and Arrived
    plt.scatter(car_load, time_traveled, label=custom_legend_name)

    plt.plot(car_load, time_traveled, marker='o', label='_nolegend_')
plt.xlabel('Car Load')
plt.ylabel('Average Time Traveled')
plt.title('Time Traveled vs. Car Load \nWith Light')

plt.ylim(ymin=0, ymax=300)


# Adjust the legend position and font size
plt.legend(bbox_to_anchor=(1.1, 1), loc='upper left', prop={'size': 8})

plt.grid()
plt.show()