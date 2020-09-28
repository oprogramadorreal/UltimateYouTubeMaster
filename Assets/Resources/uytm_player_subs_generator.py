# Gets all comments from a YouTube channel and counts the number of comments of each user.
# Learn more about YouTube API here: https://youtu.be/th5_9woFJmk

from googleapiclient.discovery import build
from collections import OrderedDict
from operator import itemgetter
import json

# Change here
api_key = 'YOUR API KEY HERE'
channel_id = 'CHANNEL ID HERE'

youtube = build('youtube', 'v3', developerKey=api_key)

results = youtube.commentThreads().list(
    part='snippet, replies',
    allThreadsRelatedToChannelId=channel_id
).execute()

data = dict()

while results:
    for item in results["items"]:
        comment = item["snippet"]["topLevelComment"]
        author = comment["snippet"]["authorDisplayName"]
        data[author] = data.get(author, 0) + 1
        print(author, " ", data[author])

    if 'nextPageToken' in results:
        results = youtube.commentThreads().list(
            part='snippet, replies',
            allThreadsRelatedToChannelId=channel_id,
            pageToken=results['nextPageToken']
        ).execute()
    else:
        break

# Sort results
sorted_data = OrderedDict(sorted(data.items(), key=itemgetter(1), reverse=True))

# Save sorted results in a JSON file
with open('uytm_player_subs.json', 'w', encoding='utf-8') as fp:
     json.dump(sorted_data, fp, ensure_ascii=False, indent=4)